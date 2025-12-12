using PeShop.Services.Admin.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Repositories;
using PeShop.Models.Enums;
using PeShop.Exceptions;
using Hangfire;
using Hangfire.Client;
using Microsoft.AspNetCore.Hosting;

namespace PeShop.Services.Admin;

public class AVoucherService : IAVoucherService
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _jobMappingFilePath;
    
    public AVoucherService(
        IVoucherRepository voucherRepository, 
        IUserRepository userRepository,
        IBackgroundJobClient backgroundJobClient,
        IWebHostEnvironment webHostEnvironment)
    {
        _voucherRepository = voucherRepository;
        _userRepository = userRepository;
        _backgroundJobClient = backgroundJobClient;
        _webHostEnvironment = webHostEnvironment;
        
        // Setup job mapping file path
        var dataPath = Path.Combine(_webHostEnvironment.ContentRootPath, "data");
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        _jobMappingFilePath = Path.Combine(dataPath, "job_mappings.json");
    }
    
    public async Task<PaginationResponse<AVoucherResponse>> GetVouchersAsync(AGetVoucherRequest request)
    {
        var vouchers = await _voucherRepository.GetListVoucherSystemAsync(request);
        var totalCount = await _voucherRepository.GetCountVoucherSystemAsync(request);
        
        // Collect all unique user IDs from CreatedBy and UpdatedBy
        var userIds = vouchers
            .Where(v => !string.IsNullOrEmpty(v.CreatedBy) || !string.IsNullOrEmpty(v.UpdatedBy))
            .SelectMany(v => new[] { v.CreatedBy, v.UpdatedBy })
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .Cast<string>()
            .ToList();
        
        // Fetch all users in parallel
        var userNamesDict = new Dictionary<string, string>();
        var userTasks = userIds.Select(userId => _userRepository.GetByIdAsync(userId)).ToList();
        var users = await Task.WhenAll(userTasks);
        
        foreach (var user in users)
        {
            if (user != null && !string.IsNullOrEmpty(user.Name))
            {
                userNamesDict[user.Id] = user.Name;
            }
        }
        
        var voucherResponses = vouchers.Select(v => new AVoucherResponse
        {
            Id = v.Id,
            Code = v.Code ?? string.Empty,
            Name = v.Name ?? string.Empty,
            Type = v.Type,
            TypeName = GetTypeName(v.Type),
            MiniumOrderValue = v.MiniumOrderValue ?? 0,
            QuantityUsed = v.QuantityUsed ?? 0,
            Quantity = v.Quantity ?? 0,
            EndTime = v.EndTime,
            Status = v.Status,
            StatusName = GetStatusName(v.Status),
            DiscountValue = v.DiscountValue ?? 0,
            MaxdiscountAmount = v.MaxdiscountAmount,
            StartTime = v.StartTime,
            CreatedAt = v.CreatedAt,
            CreatedByName = !string.IsNullOrEmpty(v.CreatedBy) && userNamesDict.ContainsKey(v.CreatedBy) ? userNamesDict[v.CreatedBy] : null,
            UpdatedAt = v.UpdatedAt,
            UpdatedByName = !string.IsNullOrEmpty(v.UpdatedBy) && userNamesDict.ContainsKey(v.UpdatedBy) ? userNamesDict[v.UpdatedBy] : null
        }).ToList();
        
        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var hasNextPage = request.Page < totalPages;
        var hasPreviousPage = request.Page > 1;

        return new PaginationResponse<AVoucherResponse>
        {
            Data = voucherResponses,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            NextPage = hasNextPage ? request.Page + 1 : request.Page,
            PreviousPage = hasPreviousPage ? request.Page - 1 : request.Page
        };
    }
    
    private string GetTypeName(VoucherValueType type)
    {
        return type switch
        {
            VoucherValueType.Percentage => "Phần trăm",
            VoucherValueType.FixedAmount => "Tiền",
            _ => "Không xác định"
        };
    }
    
    private string GetStatusName(VoucherStatus? status)
    {
        if (!status.HasValue) return "Không xác định";
        
        return status.Value switch
        {
            VoucherStatus.Inactive => "Không hoạt động",
            VoucherStatus.Active => "Đang hoạt động",
            VoucherStatus.Expired => "Hết hạn",
            _ => "Không xác định"
        };
    }

    public async Task<StatusResponse<AVoucherResponse>> CreateAsync(ACreateVoucherRequest request, string userId)
    {
        var now = DateTime.UtcNow;
        
        // Validate ngày bắt đầu và ngày kết thúc
        if (request.StartTime >= request.EndTime)
        {
            throw new BadRequestException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
        }

        if (request.EndTime <= now)
        {
            throw new BadRequestException("Ngày kết thúc phải lớn hơn thời điểm hiện tại");
        }

        // Tạo voucher entity
        var voucherId = Guid.NewGuid().ToString();
        var voucher = new Models.Entities.VoucherSystem
        {
            Id = voucherId,
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            DiscountValue = request.DiscountValue,
            MaxdiscountAmount = request.MaxdiscountAmount,
            MiniumOrderValue = request.MiniumOrderValue,
            Quantity = request.Quantity,
            QuantityUsed = 0,
            LimitForUser = request.LimitForUser,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = now,
            CreatedBy = userId,
            UpdatedAt = now,
            UpdatedBy = userId
        };

        // Xác định status dựa trên StartTime
        if (request.StartTime <= now)
        {
            // Nếu thời gian bắt đầu đã qua (hoặc bằng) hiện tại, set status là Active
            voucher.Status = VoucherStatus.Active;
            
            // Schedule job để set status = Expired khi đến EndTime
            var endDelay = request.EndTime - now;
            if (endDelay > TimeSpan.Zero)
            {
                var endJobId = $"voucherEndDate_{voucherId}";
                var endTimeUtc = request.EndTime.Kind == DateTimeKind.Utc 
                    ? request.EndTime 
                    : DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);
                var endTimeOffset = new DateTimeOffset(endTimeUtc, TimeSpan.Zero);
                var hangfireEndJobId = _backgroundJobClient.Schedule(
                    () => UpdateVoucherStatusJobAsync(voucherId, VoucherStatus.Expired),
                    endTimeOffset
                );
                SaveJobMapping(endJobId, hangfireEndJobId);
            }
        }
        else
        {
            // Nếu thời gian bắt đầu ở tương lai, set status là Inactive và schedule jobs
            voucher.Status = VoucherStatus.Inactive;
            
            // Schedule job để set status = Active khi đến StartTime
            var startJobId = $"voucherStartDate_{voucherId}";
            var startTimeUtc = request.StartTime.Kind == DateTimeKind.Utc 
                ? request.StartTime 
                : DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
            var startTimeOffset = new DateTimeOffset(startTimeUtc, TimeSpan.Zero);
            var hangfireStartJobId = _backgroundJobClient.Schedule(
                () => UpdateVoucherStatusJobAsync(voucherId, VoucherStatus.Active),
                startTimeOffset
            );
            SaveJobMapping(startJobId, hangfireStartJobId);
            
            // Schedule job để set status = Expired khi đến EndTime
            var endJobId = $"voucherEndDate_{voucherId}";
            var endTimeUtc = request.EndTime.Kind == DateTimeKind.Utc 
                ? request.EndTime 
                : DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);
            var endTimeOffset = new DateTimeOffset(endTimeUtc, TimeSpan.Zero);
            var hangfireEndJobId = _backgroundJobClient.Schedule(
                () => UpdateVoucherStatusJobAsync(voucherId, VoucherStatus.Expired),
                endTimeOffset
            );
            SaveJobMapping(endJobId, hangfireEndJobId);
        }

        // Tạo voucher trong database
        var createdVoucher = await _voucherRepository.CreateVoucherSystemAsync(voucher);

        // Map sang response
        var createdByUser = await _userRepository.GetByIdAsync(userId);
        var response = new AVoucherResponse
        {
            Id = createdVoucher.Id,
            Code = createdVoucher.Code ?? string.Empty,
            Name = createdVoucher.Name ?? string.Empty,
            Type = createdVoucher.Type,
            TypeName = GetTypeName(createdVoucher.Type),
            MiniumOrderValue = createdVoucher.MiniumOrderValue ?? 0,
            QuantityUsed = createdVoucher.QuantityUsed ?? 0,
            Quantity = createdVoucher.Quantity ?? 0,
            EndTime = createdVoucher.EndTime,
            Status = createdVoucher.Status,
            StatusName = GetStatusName(createdVoucher.Status),
            DiscountValue = createdVoucher.DiscountValue ?? 0,
            MaxdiscountAmount = createdVoucher.MaxdiscountAmount,
            StartTime = createdVoucher.StartTime,
            CreatedAt = createdVoucher.CreatedAt,
            CreatedByName = createdByUser?.Name,
            UpdatedAt = createdVoucher.UpdatedAt,
            UpdatedByName = createdByUser?.Name
        };

        return new StatusResponse<AVoucherResponse>
        {
            Status = true,
            Message = "Voucher đã được tạo thành công",
            Data = response
        };
    }

    public async Task<StatusResponse<AVoucherResponse>> UpdateAsync(string voucherId, AUpdateVoucherRequest request, string userId)
    {
        var now = DateTime.UtcNow;
        
        // Lấy voucher hiện tại
        var voucher = await _voucherRepository.GetVoucherSystemByIdAsync(voucherId);
        if (voucher == null)
        {
            throw new NotFoundException("Voucher không tồn tại");
        }

        // Kiểm tra status không được là Active hoặc Expired
        if (voucher.Status == VoucherStatus.Active || voucher.Status == VoucherStatus.Expired)
        {
            throw new BadRequestException("Không thể cập nhật voucher có trạng thái Active hoặc Expired");
        }

        // Validate ngày bắt đầu và ngày kết thúc
        if (request.StartTime >= request.EndTime)
        {
            throw new BadRequestException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
        }

        if (request.EndTime <= now)
        {
            throw new BadRequestException("Ngày kết thúc phải lớn hơn thời điểm hiện tại");
        }

        // Xóa job cũ nếu có
        var startJobId = $"voucherStartDate_{voucherId}";
        var endJobId = $"voucherEndDate_{voucherId}";
        DeleteJob(startJobId);
        DeleteJob(endJobId);

        // Update StartTime và EndTime
        voucher.StartTime = request.StartTime;
        voucher.EndTime = request.EndTime;
        voucher.UpdatedAt = now;
        voucher.UpdatedBy = userId;

        // Xác định status dựa trên StartTime
        if (request.StartTime <= now)
        {
            // Nếu thời gian bắt đầu đã qua (hoặc bằng) hiện tại, set status là Active
            voucher.Status = VoucherStatus.Active;
            
            // Schedule job để set status = Expired khi đến EndTime
            var endDelay = request.EndTime - now;
            if (endDelay > TimeSpan.Zero)
            {
                var endTimeUtc = request.EndTime.Kind == DateTimeKind.Utc 
                    ? request.EndTime 
                    : DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);
                var endTimeOffset = new DateTimeOffset(endTimeUtc, TimeSpan.Zero);
                var hangfireEndJobId = _backgroundJobClient.Schedule(
                    () => UpdateVoucherStatusJobAsync(voucherId, VoucherStatus.Expired),
                    endTimeOffset
                );
                SaveJobMapping(endJobId, hangfireEndJobId);
            }
        }
        else
        {
            // Nếu thời gian bắt đầu ở tương lai, set status là Inactive và schedule jobs
            voucher.Status = VoucherStatus.Inactive;
            
            // Schedule job để set status = Active khi đến StartTime
            var startTimeUtc = request.StartTime.Kind == DateTimeKind.Utc 
                ? request.StartTime 
                : DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
            var startTimeOffset = new DateTimeOffset(startTimeUtc, TimeSpan.Zero);
            var hangfireStartJobId = _backgroundJobClient.Schedule(
                () => UpdateVoucherStatusJobAsync(voucherId, VoucherStatus.Active),
                startTimeOffset
            );
            SaveJobMapping(startJobId, hangfireStartJobId);
            
            // Schedule job để set status = Expired khi đến EndTime
            var endTimeUtc = request.EndTime.Kind == DateTimeKind.Utc 
                ? request.EndTime 
                : DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);
            var endTimeOffset = new DateTimeOffset(endTimeUtc, TimeSpan.Zero);
            var hangfireEndJobId = _backgroundJobClient.Schedule(
                () => UpdateVoucherStatusJobAsync(voucherId, VoucherStatus.Expired),
                endTimeOffset
            );
            SaveJobMapping(endJobId, hangfireEndJobId);
        }

        // Update voucher trong database
        await _voucherRepository.UpdateVoucherSystemAsync(voucher);

        // Map sang response
        // var updatedByUser = await _userRepository.GetByIdAsync(userId);
        // var createdByUser = voucher.CreatedBy != null ? await _userRepository.GetByIdAsync(voucher.CreatedBy) : null;
        
        var response = new AVoucherResponse
        {
            Id = voucher.Id,
            Code = voucher.Code ?? string.Empty,
            Name = voucher.Name ?? string.Empty,
            Type = voucher.Type,
            TypeName = GetTypeName(voucher.Type),
            MiniumOrderValue = voucher.MiniumOrderValue ?? 0,
            QuantityUsed = voucher.QuantityUsed ?? 0,
            Quantity = voucher.Quantity ?? 0,
            EndTime = voucher.EndTime,
            Status = voucher.Status,
            StatusName = GetStatusName(voucher.Status),
            DiscountValue = voucher.DiscountValue ?? 0,
            MaxdiscountAmount = voucher.MaxdiscountAmount,
            StartTime = voucher.StartTime,
            CreatedAt = voucher.CreatedAt,
            // CreatedByName = createdByUser?.Name,
            UpdatedAt = voucher.UpdatedAt,
            // UpdatedByName = updatedByUser?.Name
        };

        return new StatusResponse<AVoucherResponse>
        {
            Status = true,
            Message = "Voucher đã được cập nhật thành công",
            Data = response
        };
    }

    public async Task<StatusResponse> UpdateVoucherStatusJobAsync(string voucherId, VoucherStatus status)
    {
        var voucher = await _voucherRepository.GetVoucherSystemByIdAsync(voucherId);
        if (voucher == null)
        {
            return new StatusResponse
            {
                Status = false,
                Message = "Voucher không tồn tại"
            };
        }

        voucher.Status = status;
        voucher.UpdatedAt = DateTime.UtcNow;

        var updated = await _voucherRepository.UpdateVoucherSystemAsync(voucher);
        if (!updated)
        {
            return new StatusResponse
            {
                Status = false,
                Message = "Không thể cập nhật trạng thái voucher"
            };
        }

        return new StatusResponse
        {
            Status = true,
            Message = $"Đã cập nhật trạng thái voucher thành {GetStatusName(status)}"
        };
    }

    private void SaveJobMapping(string customId, string hangfireJobId)
    {
        try
        {
            var mappings = LoadJobMappings();
            mappings[customId] = hangfireJobId;

            var json = System.Text.Json.JsonSerializer.Serialize(mappings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_jobMappingFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving job mapping: {ex.Message}");
        }
    }

    private Dictionary<string, string> LoadJobMappings()
    {
        try
        {
            if (File.Exists(_jobMappingFilePath))
            {
                var json = File.ReadAllText(_jobMappingFilePath);
                var mappings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return mappings ?? new Dictionary<string, string>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading job mappings: {ex.Message}");
        }
        return new Dictionary<string, string>();
    }

    private string? GetHangfireJobId(string customId)
    {
        try
        {
            var mappings = LoadJobMappings();
            return mappings.TryGetValue(customId, out var jobId) ? jobId : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting job mapping: {ex.Message}");
            return null;
        }
    }

    private void RemoveJobMapping(string customId)
    {
        try
        {
            var mappings = LoadJobMappings();
            if (mappings.Remove(customId))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(mappings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_jobMappingFilePath, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing job mapping: {ex.Message}");
        }
    }

    private void DeleteJob(string jobId)
    {
        if (string.IsNullOrEmpty(jobId))
        {
            return;
        }

        // Tìm Hangfire JobId từ custom ID
        var hangfireJobId = GetHangfireJobId(jobId);

        if (!string.IsNullOrEmpty(hangfireJobId))
        {
            // Xóa job bằng Hangfire job ID
            BackgroundJob.Delete(hangfireJobId);
            // Xóa mapping khỏi file
            RemoveJobMapping(jobId);
            Console.WriteLine($"Deleted job with custom ID: {jobId}, Hangfire JobId: {hangfireJobId}");
        }
        else
        {
            // Nếu không tìm thấy mapping, coi như jobId là Hangfire JobId trực tiếp
            BackgroundJob.Delete(jobId);
            Console.WriteLine($"Deleted job with Hangfire JobId: {jobId}");
        }
    }

    public async Task<StatusResponse> DeleteAsync(string voucherId)
    {
        // Kiểm tra voucher có tồn tại không
        var voucher = await _voucherRepository.GetVoucherSystemByIdAsync(voucherId);
        if (voucher == null)
        {
            throw new NotFoundException("Voucher không tồn tại");
        }

        // Xóa jobs liên quan nếu có
        var startJobId = $"voucherStartDate_{voucherId}";
        var endJobId = $"voucherEndDate_{voucherId}";
        DeleteJob(startJobId);
        DeleteJob(endJobId);

        // Thử xóa voucher
        var success = await _voucherRepository.DeleteVoucherSystemAsync(voucherId);
        
        if (!success)
        {
            // Xóa thất bại do foreign key constraint (đã có người sử dụng)
            throw new BadRequestException("Không thể xóa voucher vì đã có người sử dụng. Chỉ có thể kết thúc voucher bằng cách set status = Expired");
        }

        return new StatusResponse
        {
            Status = true,
            Message = "Voucher đã được xóa thành công"
        };
    }

    public async Task<StatusResponse> SetExpiredAsync(string voucherId)
    {
        var voucher = await _voucherRepository.GetVoucherSystemByIdAsync(voucherId);
        if (voucher == null)
        {
            throw new NotFoundException("Voucher không tồn tại");
        }

        // Xóa jobs liên quan nếu có
        var startJobId = $"voucherStartDate_{voucherId}";
        var endJobId = $"voucherEndDate_{voucherId}";
        DeleteJob(startJobId);
        DeleteJob(endJobId);

        // Set status = Expired
        voucher.Status = VoucherStatus.Expired;
        voucher.UpdatedAt = DateTime.UtcNow;

        var updated = await _voucherRepository.UpdateVoucherSystemAsync(voucher);
        if (!updated)
        {
            return new StatusResponse
            {
                Status = false,
                Message = "Không thể cập nhật trạng thái voucher"
            };
        }

        return new StatusResponse
        {
            Status = true,
            Message = "Voucher đã được kết thúc thành công"
        };
    }
}


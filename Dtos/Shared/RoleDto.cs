namespace PeShop.Dtos.Shared;

public class RoleDto
{
    public string Id { get; set; } = null!;
    public string? Code { get; set; } // Tên code của role (ví dụ: "Admin")
    public string? Name { get; set; } // Display name của role (ví dụ: "Admin (Chủ doanh nghiệp)")
    public DateTime? CreatedAt { get; set; }
    public string? CreatedByName { get; set; } // Tên người tạo
    public string? UpdatedByName { get; set; } // Tên người cập nhật
    public List<string> ListPermission { get; set; } = []; // Danh sách permissions
}

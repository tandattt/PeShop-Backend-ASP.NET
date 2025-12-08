namespace PeShop.Dtos.Responses;

/// <summary>
/// Response DTO for a single permission
/// </summary>
public class PermissionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? Action { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
public class PermissionGroupedResponse
{
    public Dictionary<string, List<PermissionResponse>> PermissionsByModule { get; set; } = new();
}

public class PermissionsByModuleResponse
{
    public string Module { get; set; } = string.Empty;
    public List<PermissionResponse> Permissions { get; set; } = new();
}

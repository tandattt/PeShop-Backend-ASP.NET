namespace PeShop.Dtos.Shared
{
    public class PropertyForCartDto
    {
        public string Name { get; set; } = string.Empty;
    }
    public class PropertyForProductDto : PropertyForCartDto
    {
        // public string Id { get; set; } = string.Empty;
    }
    public class PropertyDto : PropertyForProductDto
    {
        public string Value { get; set; } = string.Empty;
    }
}
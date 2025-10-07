namespace PeShop.Dtos.Shared
{
    public class PropertyValueForCartDto
    {
        public string ImgUrl { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Value { get; set; } = string.Empty;
    }
    public class PropertyValueForProductDto : PropertyValueForCartDto
    {
        public string PropertyName { get; set; } = string.Empty;
    }
    public class PropertyValueDto : PropertyValueForProductDto
    {
        public string PropertyValueName { get; set; } = string.Empty;
    }
}
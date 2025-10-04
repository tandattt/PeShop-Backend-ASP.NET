namespace PeShop.Dtos.Shared
{
    public class PropertyValueForCart
    {
        public string ImgUrl { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Value { get; set; } = string.Empty;
    }
    public class PropertyValue : PropertyValueForCart
    {
        public string Id { get; set; } = string.Empty;
        public string PropertyProductId { get; set; } = string.Empty;
        public string PropertyId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyValueId { get; set; } = string.Empty;
        public string PropertyValueName { get; set; } = string.Empty;
    }
}
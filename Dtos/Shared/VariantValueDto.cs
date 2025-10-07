namespace PeShop.Dtos.Shared
{
    public class VariantValueForCartDto
    {
        public PropertyValueForCartDto PropertyValue { get; set; } = new PropertyValueForCartDto();
        public PropertyForCartDto Property { get; set; } = new PropertyForCartDto();
    }
    public class VariantValueForProductDto
    {
        public PropertyValueForProductDto PropertyValue { get; set; } = new PropertyValueForProductDto();
        public PropertyForProductDto Property { get; set; } = new PropertyForProductDto();
    }
    public class VariantValue : VariantValueForCartDto
    {
        public string Id { get; set; } = string.Empty;
    }
}
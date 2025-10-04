namespace PeShop.Dtos.Shared
{
    public class PropertyForCart
    {
        public string Name { get; set; } = string.Empty;
    }
    public class Property : PropertyForCart
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
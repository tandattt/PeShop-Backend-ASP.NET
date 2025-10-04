namespace PeShop.Dtos.Shared
{
    public class VariantValueForCart
    {
        public PropertyValueForCart PropertyValue { get; set; } = new PropertyValueForCart();
        public PropertyForCart Property { get; set; } = new PropertyForCart();
    }
    // public class VariantValue : VariantValueForCart
    // {
    //     public string Id { get; set; } = string.Empty;
    // }
}
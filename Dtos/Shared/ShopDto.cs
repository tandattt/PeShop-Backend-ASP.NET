namespace PeShop.Dtos.Shared
{
    public class ShopDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string NewProviceId { get; set; } = string.Empty;
        public uint ProductCount { get; set; } = 0;
        public uint FollowersCount { get; set; } = 0;
    }
 
    
}

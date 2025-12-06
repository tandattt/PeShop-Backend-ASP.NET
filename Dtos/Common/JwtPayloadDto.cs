namespace PeShop.Dtos.Common
{
    public class JwtPayloadDto
    {
        public string Sub { get; set; }
        public string? ShopId { get; set; }
        public List<string> Authorities { get; set; }
        public List<string>? Permissions { get; set; }
        public string TokenType { get; set; }
        public int TimeLive { get; set; }
    }
}

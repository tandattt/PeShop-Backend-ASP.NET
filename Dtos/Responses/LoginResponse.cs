namespace PeShop.Dtos.Responses
{
    /// <summary>
    /// Response DTO cho login
    /// </summary>
    public class LoginResponse
    {

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}

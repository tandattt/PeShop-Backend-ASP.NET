namespace PeShop.Dtos.Responses
{
    public class VerifyOtpResponse : StatusResponse
    {
        public string Key { get; set; } = string.Empty;
    }
}
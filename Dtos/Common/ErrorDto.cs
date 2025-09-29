namespace PeShop.Dtos.Common
{
    public class ErrorDto
    {
        public string Message { get; set; }
    }
    public class SystemErrorDto : ErrorDto
    {
        public string Detail { get; set; }
    }
}

namespace PeShop.Dtos.Responses
{
    public class StatusResponse
    {
        public bool Status { get; set; } = false;
        public string? Message { get; set; } = null;
    }
    public class StatusResponse<T> : StatusResponse
    {
        public T? Data { get; set; } = default(T);
    }
}
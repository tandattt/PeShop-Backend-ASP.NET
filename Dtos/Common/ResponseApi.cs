namespace PeShop.Dtos.Common
{
    public class ResponseApi<T> where T : class
    {
        public T? Error { get; set; }
        public T? Data { get; set; }
        public static ResponseApi<T> Success(T data) => new ResponseApi<T> { Data = data, Error = null};
        public static ResponseApi<T> Fail(T error) => new ResponseApi<T> { Error = error , Data = null};
    }
}

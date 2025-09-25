namespace API.Commons
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public T? Data { get; private set; }

        private ServiceResult(bool isSuccess, T? data, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult<T> Success(T data) => new(true, data, null);

        public static ServiceResult<T> Fail(string errorMessage) => new(false, default, errorMessage);
    }

}

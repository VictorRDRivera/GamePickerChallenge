namespace GamePicker.Contracts
{
    public record ApiResponse<T>
    {
        public T? Data { get; init; }
        public string? Message { get; init; }
        public List<string>? Errors { get; init; }
        public int StatusCode { get; init; }

        public static ApiResponse<T> CreateSuccess(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }

        public static ApiResponse<T> CreateError(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }
    }

    public record ApiResponse
    {
        public object? Data { get; init; }
        public string? Message { get; init; }
        public List<string>? Errors { get; init; }
        public int StatusCode { get; init; }

        public static ApiResponse CreateSuccess(object? data = null, string? message = null)
        {
            return new ApiResponse
            {
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }

        public static ApiResponse CreateError(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new ApiResponse
            {
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }
    }
}

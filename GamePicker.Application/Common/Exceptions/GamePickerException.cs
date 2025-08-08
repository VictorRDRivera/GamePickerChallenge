namespace GamePicker.Application.Common.Exceptions
{
    public abstract class GamePickerException : Exception
    {
        public int StatusCode { get; }

        protected GamePickerException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class ValidationException : GamePickerException
    {
        public List<string> Errors { get; }

        public ValidationException(string message, List<string> errors) : base(message, 400)
        {
            Errors = errors;
        }
    }

    public class NotFoundException : GamePickerException
    {
        public NotFoundException(string message) : base(message, 404)
        {
        }
    }

    public class UnauthorizedException : GamePickerException
    {
        public UnauthorizedException(string message) : base(message, 401)
        {
        }
    }

    public class ForbiddenException : GamePickerException
    {
        public ForbiddenException(string message) : base(message, 403)
        {
        }
    }

    public class InternalServerException : GamePickerException
    {
        public InternalServerException(string message) : base(message, 500)
        {
        }
    }

    public class ExternalApiException : GamePickerException
    {
        public ExternalApiException(string message) : base(message, 502)
        {
        }
    }
}

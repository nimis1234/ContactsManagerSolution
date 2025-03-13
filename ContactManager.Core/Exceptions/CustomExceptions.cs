namespace Exceptions
{
    public class CustomExceptions : Exception
    {
        // this is used to handle the exception occurs in business logic and give a custom message to the user
        // catch method insted of throw excpetion throw custom exception()
        // 
        public CustomExceptions(string message) : base(message) { }


        public CustomExceptions(string message, Exception innerException) : base(message, innerException) { }
        public CustomExceptions(string message, string errorCode, Exception innerException)
            : base($"{message} (Error Code: {errorCode})", innerException) { }

        public CustomExceptions() : base() { }
    }
}

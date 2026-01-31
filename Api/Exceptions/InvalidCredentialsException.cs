namespace Api.Exceptions
{
    public sealed class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base("Invalid credentials.") { }
    }

    public sealed class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }


    public sealed class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

}

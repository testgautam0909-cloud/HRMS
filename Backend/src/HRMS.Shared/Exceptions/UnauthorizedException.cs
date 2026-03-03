namespace HRMS.Shared.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Authentication is required to access this resource.")
        : base(message)
    {
    }
}

namespace AuthService.Exceptions
{
    public class IncorrectLoginOrPasswordException(string message = "Incorrect login or password") : Exception(message);
}
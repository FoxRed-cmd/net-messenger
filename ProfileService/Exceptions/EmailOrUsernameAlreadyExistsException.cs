namespace ProfileService.Exceptions
{
    public class EmailOrUsernameAlreadyExistsException(string message) : Exception(message);
}
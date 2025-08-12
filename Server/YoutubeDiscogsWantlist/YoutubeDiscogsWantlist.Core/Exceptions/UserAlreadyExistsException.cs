namespace YoutubeDiscogsWantlist.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string username)
        : base($"A user with the username '{username}' already exists.")
    {
        Username = username;
    }

    public string Username { get; }
}
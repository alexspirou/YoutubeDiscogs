namespace YoutubeDiscogsWantlist.Exceptions;

public class DiscogsUserNotFoundException : Exception
{
    public DiscogsUserNotFoundException(string username)
        : base($"User with username '{username}' not found.") { }
}
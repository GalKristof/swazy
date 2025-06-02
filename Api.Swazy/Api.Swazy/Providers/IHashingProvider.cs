namespace Api.Swazy.Providers;

public interface IHashingProvider
{
    public string HashPassword(string password);

    public bool ValidatePassword(string password, string correctHash);
}
namespace Api.Swazy.Common;

public static class ConfirmationCodeGenerator
{
    private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Excluding similar characters (I, O, 0, 1)
    private const int CodeLength = 6;

    public static string Generate()
    {
        var random = new Random();
        var code = new char[CodeLength];

        for (int i = 0; i < CodeLength; i++)
        {
            code[i] = Characters[random.Next(Characters.Length)];
        }

        return new string(code);
    }
}

public static class PasswordUtils
{
    private const int MinLength = 8;
    private const char PadChar = '_';

    public static string Normalize(string rawPassword)
    {
        if (string.IsNullOrEmpty(rawPassword))
            return null; // no password — stays null, unaffected

        return rawPassword.Length >= MinLength
            ? rawPassword
            : rawPassword.PadRight(MinLength, PadChar);
    }
}
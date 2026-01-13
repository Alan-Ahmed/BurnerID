namespace Domain.Common;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} must not be null/empty.");
        return value;
    }

    public static int InRange(int value, int min, int max, string name)
    {
        if (value < min || value > max)
            throw new DomainException($"{name} must be between {min} and {max}.");
        return value;
    }

    public static byte[] NotNullOrEmpty(byte[]? value, string name)
    {
        if (value is null || value.Length == 0)
            throw new DomainException($"{name} must not be null/empty.");
        return value;
    }
}

using Domain.Common;

namespace Domain.ValueObjects;

public readonly record struct UserId
{
    public string Value { get; }

    public UserId(string value)
    {
        Value = Guard.NotNullOrWhiteSpace(value, nameof(UserId));
        Guard.InRange(Value.Length, 6, 200, nameof(UserId));
    }

    public override string ToString() => Value;

    public static UserId From(string value) => new(value);
}

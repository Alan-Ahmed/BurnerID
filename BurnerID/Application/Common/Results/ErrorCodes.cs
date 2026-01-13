namespace Application.Common.Results;

public static class ErrorCodes
{
    public const string Validation = "validation_error";
    public const string NotFound = "not_found";
    public const string Unauthorized = "unauthorized";
    public const string RateLimited = "rate_limited";
    public const string Conflict = "conflict";
    public const string Crypto = "crypto_error";
    public const string Internal = "internal_error";
}

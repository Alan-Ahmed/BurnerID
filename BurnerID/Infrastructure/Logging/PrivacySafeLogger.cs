using Application.Common.Abstractions;
using Infrastructure.Logging.Hashing;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

public sealed class PrivacySafeLogger : IPrivacySafeLogger
{
    private readonly ILogger<PrivacySafeLogger> _logger;

    public PrivacySafeLogger(ILogger<PrivacySafeLogger> logger)
    {
        _logger = logger;
    }

    public void Info(string messageTemplate, params object[] args)
        => _logger.LogInformation(Sanitize(messageTemplate, args));

    public void Warn(string messageTemplate, params object[] args)
        => _logger.LogWarning(Sanitize(messageTemplate, args));

    public void Error(Exception ex, string messageTemplate, params object[] args)
        => _logger.LogError(ex, Sanitize(messageTemplate, args));

    private static string Sanitize(string template, object[] args)
    {
        // Privacy-first: we hash any arg that "looks like" an identifier (string length 6+)
        // and keep ints (like payload sizes) unchanged.
        var safeArgs = args.Select(a =>
        {
            if (a is string s && s.Length >= 6)
                return $"hash:{Sha256Hasher.HashToHex(s).Substring(0, 12)}";
            return a;
        }).ToArray();

        // We format the message ourselves to avoid structured logging leaking raw args.
        try
        {
            return string.Format(template, safeArgs);
        }
        catch
        {
            return template; // fallback
        }
    }
}

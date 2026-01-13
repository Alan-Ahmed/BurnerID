using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Models;

public sealed class Envelope
{
    public string EnvelopeId { get; }
    public UserId From { get; }
    public UserId To { get; }

    public byte[] Ciphertext { get; }

    public string? ContentType { get; }
    public string? AlgoVersion { get; }
    public DateTimeOffset CreatedAt { get; }

    public Envelope(
        string envelopeId,
        UserId from,
        UserId to,
        byte[] ciphertext,
        DateTimeOffset createdAt,
        string? contentType = null,
        string? algoVersion = null)
    {
        EnvelopeId = Guard.NotNullOrWhiteSpace(envelopeId, nameof(envelopeId));
        From = from;
        To = to;
        Ciphertext = Guard.NotNullOrEmpty(ciphertext, nameof(ciphertext));
        CreatedAt = createdAt;

        ContentType = contentType;
        AlgoVersion = algoVersion;
    }

    public int PayloadSizeBytes => Ciphertext.Length;
}

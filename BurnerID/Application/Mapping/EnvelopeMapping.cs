using Application.Dtos;
using Domain.Models;
using Domain.ValueObjects;

namespace Application.Mapping;

public static class EnvelopeMapping
{
    public static Envelope ToDomain(this EnvelopeDto dto, DateTimeOffset nowUtc)
        => new(
            envelopeId: dto.EnvelopeId,
            from: UserId.From(dto.FromUserId),
            to: UserId.From(dto.ToUserId),
            ciphertext: Base64UrlDecode(dto.CiphertextBase64Url),
            createdAt: nowUtc,
            contentType: dto.ContentType,
            algoVersion: dto.AlgoVersion
        );

    private static byte[] Base64UrlDecode(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}

using Application.Common.Abstractions;
using Application.Contracts;
using NSec.Cryptography;

namespace Infrastructure.Crypto;

public sealed class Ed25519CryptoVerifier : ICryptoVerifier
{
    public bool VerifyEd25519(
        ReadOnlySpan<byte> message,
        ReadOnlySpan<byte> signature,
        ReadOnlySpan<byte> publicKey)
    {
        var alg = SignatureAlgorithm.Ed25519;

        // NSec vill ha byte[] när man importerar nyckeln
        var pk = PublicKey.Import(
            alg,
            publicKey.ToArray(),
            KeyBlobFormat.RawPublicKey);

        // NSec Verify tar byte[]
        return alg.Verify(pk, message.ToArray(), signature.ToArray());
    }
}

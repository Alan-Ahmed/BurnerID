namespace Application.Contracts;

public interface ICryptoVerifier
{
    bool VerifyEd25519(ReadOnlySpan<byte> publicKeyBytes, ReadOnlySpan<byte> messageBytes, ReadOnlySpan<byte> signatureBytes);
}

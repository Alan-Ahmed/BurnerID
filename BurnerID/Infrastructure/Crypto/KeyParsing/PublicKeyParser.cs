using Infrastructure.Crypto.KeyParsing;

namespace Infrastructure.Crypto.KeyParsing;

public static class PublicKeyParser
{
    // For demo: public key provided as base64url raw 32-byte Ed25519 public key
    public static byte[] ParseEd25519PublicKey(string publicKeyBase64Url)
    {
        var key = Base64Url.Decode(publicKeyBase64Url);
        if (key.Length != 32)
            throw new ArgumentException("Ed25519 public key must be 32 bytes.");
        return key;
    }
}

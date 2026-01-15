using Application.Common.Abstractions;
using Application.Common.Results;
using Application.Contracts;
using Application.UseCases.AuthenticateConnection;
using Domain.Models;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests;

public class AuthenticateConnectionHandlerTests
{
    public class FakeCryptoVerifier : ICryptoVerifier
    {
        public bool ShouldPass { get; set; } = true;

        public bool VerifyEd25519(ReadOnlySpan<byte> publicKeyBytes, ReadOnlySpan<byte> messageBytes, ReadOnlySpan<byte> signatureBytes)
        {
            return ShouldPass;
        }
    }

    [Fact]
    public async Task Handle_ShouldAuthenticate_WhenSignatureIsValid()
    {
        // --- 1. ARRANGE ---
        var mockStore = new Mock<IChallengeStore>();
        var mockRegistry = new Mock<IConnectionRegistry>();
        var mockClock = new Mock<IClock>();
        var mockLogger = new Mock<IPrivacySafeLogger>();

        var fakeCrypto = new FakeCryptoVerifier { ShouldPass = true };

        var userIdStr = "TestUser_123";
        var challengeId = "chal_123";
        var connId = "conn_99";
        var validBase64 = "AQIDBA==";

        var now = DateTimeOffset.UtcNow;
        mockClock.Setup(c => c.UtcNow).Returns(now.DateTime);

        var challenge = new Challenge(challengeId, new byte[] { 1, 2, 3 }, now.AddMinutes(5));

        mockStore
            .Setup(x => x.GetAsync(It.IsAny<UserId>(), challengeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        mockStore
            .Setup(x => x.ConsumeAsync(It.IsAny<UserId>(), challengeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AuthenticateConnectionHandler(
            mockStore.Object,
            mockRegistry.Object,
            fakeCrypto,
            mockClock.Object,
            mockLogger.Object
        );

        var command = new AuthenticateConnectionCommand(
            connId, userIdStr, challengeId, validBase64, validBase64);

        // --- 2. ACT ---
        var result = await handler.Handle(command, CancellationToken.None);

        // --- 3. ASSERT ---
        Assert.True(result.IsSuccess);

        // FIX: Lade till '!' efter result.Value för att tysta varningen
        Assert.True(result.Value!.Authenticated);

        mockRegistry.Verify(x => x.MarkAuthenticatedAsync(
            connId,
            It.Is<UserId>(u => u.Value == userIdStr),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSignatureIsInvalid()
    {
        // --- ARRANGE ---
        var mockStore = new Mock<IChallengeStore>();
        var mockRegistry = new Mock<IConnectionRegistry>();
        var mockClock = new Mock<IClock>();
        var mockLogger = new Mock<IPrivacySafeLogger>();

        var fakeCrypto = new FakeCryptoVerifier { ShouldPass = false };

        var now = DateTimeOffset.UtcNow;
        mockClock.Setup(c => c.UtcNow).Returns(now.DateTime);

        var challenge = new Challenge("chal_123", new byte[] { 1, 2, 3 }, now.AddMinutes(5));

        mockStore
            .Setup(x => x.GetAsync(It.IsAny<UserId>(), "chal_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        var handler = new AuthenticateConnectionHandler(
            mockStore.Object,
            mockRegistry.Object,
            fakeCrypto,
            mockClock.Object,
            mockLogger.Object
        );

        var command = new AuthenticateConnectionCommand(
            "conn_1", "User_X", "chal_123", "AQID", "AQID");

        // --- ACT ---
        var result = await handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        Assert.False(result.IsSuccess);

        // FIX: Lade till '!' efter result.Error för att tysta varningen
        Assert.Equal(ErrorCodes.Unauthorized, result.Error!.Code);

        mockRegistry.Verify(x => x.MarkAuthenticatedAsync(
            It.IsAny<string>(),
            It.IsAny<UserId>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
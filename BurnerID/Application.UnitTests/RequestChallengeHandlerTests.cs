using Application.Common.Abstractions;
using Application.Common.Options;
using Application.Contracts;
using Application.UseCases.RequestChallenge;
using Domain.Models;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests;

public class RequestChallengeHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // --- 1. ARRANGE ---
        var mockStore = new Mock<IChallengeStore>();
        var mockClock = new Mock<IClock>();
        var mockLogger = new Mock<IPrivacySafeLogger>();

        // Vi sätter en fast tid för testet
        var now = DateTimeOffset.UtcNow;
        mockClock.Setup(c => c.UtcNow).Returns(now.DateTime);

        // FIX 1: Vi skapar tomma inställningar för att slippa gissa vad egenskapen heter.
        // Handlern kommer använda sina standardvärden.
        var options = new SecurityOptions();

        var userIdStr = "TestUser_123";

        mockStore
            .Setup(x => x.IssueAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserId uid, CancellationToken ct) =>
            {
                // FIX 2: Här är den korrekta konstruktorn baserat på din kod!
                // Argument 1: ChallengeId (string)
                // Argument 2: Nonce (byte[] - OBS: En array av bytes, inte en sträng)
                // Argument 3: ExpiresAt (DateTimeOffset)
                return new Challenge(
                    "test_challenge_id",
                    new byte[] { 1, 2, 3, 4 },
                    now.AddMinutes(5)
                );
            });

        var handler = new RequestChallengeHandler(
            mockStore.Object,
            mockClock.Object,
            options,
            mockLogger.Object
        );

        var command = new RequestChallengeCommand(userIdStr);

        // --- 2. ACT ---
        var result = await handler.Handle(command, CancellationToken.None);

        // --- 3. ASSERT ---
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Vi kollar att vi fick tillbaka ID:t vi skapade i mocken
        Assert.Equal("test_challenge_id", result.Value.Challenge.ChallengeId);

        // Vi verifierar att databasen anropades med rätt UserID
        mockStore.Verify(x => x.IssueAsync(
            It.Is<UserId>(u => u.Value == userIdStr),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
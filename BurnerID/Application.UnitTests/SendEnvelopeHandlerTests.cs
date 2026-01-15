using Application.Common.Abstractions;
using Application.Common.Results;
using Application.Contracts;
using Application.Dtos;
using Application.UseCases.SendEnvelope;
using Domain.Models;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests;

public class SendEnvelopeHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRouteEnvelope_WhenRequestIsValid()
    {
        // --- 1. ARRANGE ---
        var mockLimiter = new Mock<IRateLimiter>();
        var mockRouter = new Mock<IEnvelopeRouter>();
        var mockRegistry = new Mock<IConnectionRegistry>();
        var mockClock = new Mock<IClock>();
        var mockLogger = new Mock<IPrivacySafeLogger>();

        // Sätt tiden
        var now = DateTimeOffset.UtcNow;
        mockClock.Setup(c => c.UtcNow).Returns(now.DateTime);

        // Skapa handlern med exakt de 5 beroenden som din kod kräver
        var handler = new SendEnvelopeHandler(
            mockLimiter.Object,
            mockRouter.Object,
            mockRegistry.Object,
            mockClock.Object,
            mockLogger.Object
        );

        // Testdata
        var senderId = "User_Sender";
        var receiverId = "User_Receiver";
        var connectionId = "conn_123";
        var ip = "127.0.0.1";

        // MOCKA AUTENTISERING:
        mockRegistry
            .Setup(x => x.GetAuthenticatedUserAsync(connectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserId.From(senderId));

        // MOCKA RATE LIMITER:
        mockLimiter.Setup(x => x.AllowIpAsync(ip, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockLimiter.Setup(x => x.AllowUserAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Skapa DTO:n
        // FIX: Ändrade sista argumentet från 1 (int) till "1" (string)
        var envelopeDto = new EnvelopeDto(
            Guid.NewGuid().ToString(),
            senderId,
            receiverId,
            "SGVsbG8gV29ybGQ=",
            "text/plain",
            "1"  // <--- Ändrat till sträng här!
        );

        var command = new SendEnvelopeCommand(
            connectionId,
            senderId,
            envelopeDto,
            ip
        );

        // --- 2. ACT ---
        var result = await handler.Handle(command, CancellationToken.None);

        // --- 3. ASSERT ---
        Assert.True(result.IsSuccess, "Det borde gå att skicka meddelandet");

        // Verifiera att Routern anropades
        // FIX: Använder e.From.Value och e.To.Value istället för FromUserId/ToUserId
        mockRouter.Verify(x => x.DeliverAsync(
            It.Is<UserId>(u => u.Value == receiverId),
            It.Is<Envelope>(e => e.From.Value == senderId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRateLimited()
    {
        // --- ARRANGE ---
        var mockLimiter = new Mock<IRateLimiter>();
        var mockRouter = new Mock<IEnvelopeRouter>();
        var mockRegistry = new Mock<IConnectionRegistry>();
        var mockClock = new Mock<IClock>();
        var mockLogger = new Mock<IPrivacySafeLogger>();

        var senderId = "User_Spammer";
        var connectionId = "conn_spam";

        mockRegistry
            .Setup(x => x.GetAuthenticatedUserAsync(connectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserId.From(senderId));

        // Rate Limiter säger NEJ
        mockLimiter
            .Setup(x => x.AllowIpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SendEnvelopeHandler(
            mockLimiter.Object, mockRouter.Object, mockRegistry.Object, mockClock.Object, mockLogger.Object);

        // FIX: Ändrat sista argumentet till "1" här också
        var envelopeDto = new EnvelopeDto(
            Guid.NewGuid().ToString(), senderId, "User_Receiver", "data", "text", "1");

        var command = new SendEnvelopeCommand(connectionId, senderId, envelopeDto, "1.2.3.4");

        // --- ACT ---
        var result = await handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.RateLimited, result.Error!.Code);

        // Se till att vi INTE skickade något
        mockRouter.Verify(x => x.DeliverAsync(It.IsAny<UserId>(), It.IsAny<Envelope>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
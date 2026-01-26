using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace BurnerBackend.Hubs
{
    public class EnvelopeDto
    {
        public string EnvelopeId { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string CiphertextBase64Url { get; set; }
        public string ContentType { get; set; }
        public string AlgoVersion { get; set; }
        public int? BurnAfterSeconds { get; set; }
    }

    public class BurnHub : Hub
    {
        // Vi sparar en referens till "Context" som lever för evigt (Singleton)
        private readonly IHubContext<BurnHub> _hubContext;

        // DI-containern injicerar contexten här automatiskt
        public BurnHub(IHubContext<BurnHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task<string> JoinIdentity(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"[HUB] User joined: {userId} (ConnectionId: {Context.ConnectionId})");
            return Guid.NewGuid().ToString();
        }

        public async Task<bool> VerifyIdentity(string userId, string signature)
        {
            Console.WriteLine($"[HUB] User verified: {userId}");
            return true;
        }

        public async Task SendEnvelope(EnvelopeDto env)
        {
            // Logga vad som händer (Visar nu "10s" eller "0s")
            var burnText = env.BurnAfterSeconds.HasValue ? $"{env.BurnAfterSeconds}s" : "No burn";
            Console.WriteLine($"[HUB] Message from {env.FromUserId} to {env.ToUserId}. Burn: {burnText}");

            // 1. Skicka meddelandet via den vanliga "Clients" (den lever just nu)
            await Clients.Group(env.ToUserId).SendAsync("ReceiveEnvelope", env);

            // 2. Starta bakgrundstimer (om timer finns)
            if (env.BurnAfterSeconds.HasValue && env.BurnAfterSeconds.Value > 0)
            {
                // Vi använder Task.Run för att släppa loss tråden helt från Hubben
                _ = Task.Run(async () =>
                {
                    // Vänta i X sekunder
                    await Task.Delay(env.BurnAfterSeconds.Value * 1000);

                    try
                    {
                        // HÄR ÄR FIXEN: Vi använder _hubContext istället för Clients!
                        // _hubContext lever även efter att denna request är klar.
                        
                        await _hubContext.Clients.Group(env.ToUserId).SendAsync("DeleteEnvelope", env.EnvelopeId);
                        await _hubContext.Clients.Group(env.FromUserId).SendAsync("DeleteEnvelope", env.EnvelopeId);

                        Console.WriteLine($"[HUB] Burned message {env.EnvelopeId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[HUB] Error burning message: {ex.Message}");
                    }
                });
            }
        }
    }
}

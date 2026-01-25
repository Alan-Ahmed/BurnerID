using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace BurnerBackend.Hubs // <-- VIKTIGT: Behåll ditt namespace om det heter något annat!
{
    // Denna klass måste matcha datan som skickas från React
    public class EnvelopeDto
    {
        public string EnvelopeId { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string CiphertextBase64Url { get; set; }
        public string ContentType { get; set; }
        public string AlgoVersion { get; set; }
    }

    public class BurnHub : Hub
    {
        // 1. Inloggning: Kopplar ihop användarens ID med SignalR-uppkopplingen
        public async Task<string> JoinIdentity(string userId)
        {
            // Vi skapar en "Grupp" med namnet på användarens ID.
            // Detta gör att vi senare kan skicka meddelanden till "ghost-1234".
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            Console.WriteLine($"[HUB] User joined: {userId} (ConnectionId: {Context.ConnectionId})");

            // Returnera en "utmaning" (används inte på riktigt än, men krävs av frontend)
            return Guid.NewGuid().ToString();
        }

        // 2. Verifiering: Frontend bekräftar att den är redo
        public async Task<bool> VerifyIdentity(string userId, string signature)
        {
            Console.WriteLine($"[HUB] User verified: {userId}");
            return true; // Vi godkänner alla just nu
        }

        // 3. Skicka meddelande: Tar emot från avsändaren och skickar till mottagaren
        public async Task SendEnvelope(EnvelopeDto env)
        {
            Console.WriteLine($"[HUB] Message from {env.FromUserId} to {env.ToUserId}");

            // Skicka meddelandet BARA till den specifika mottagaren (Group)
            // Metodnamnet "ReceiveEnvelope" måste matcha det vi lyssnar på i React (signalr.ts)
            await Clients.Group(env.ToUserId).SendAsync("ReceiveEnvelope", env);
        }
    }
}
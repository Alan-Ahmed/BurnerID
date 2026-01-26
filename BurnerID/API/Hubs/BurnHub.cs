using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace BurnerBackend.Hubs 
{
    // Uppdaterad DTO som nu inkluderar timer för radering
    public class EnvelopeDto
    {
        public string EnvelopeId { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string CiphertextBase64Url { get; set; }
        public string ContentType { get; set; }
        public string AlgoVersion { get; set; }
        
        // NYTT: Denna låter frontend berätta hur länge meddelandet ska leva (i sekunder)
        public int? BurnAfterSeconds { get; set; }
    }

    public class BurnHub : Hub
    {
        // 1. Inloggning
        public async Task<string> JoinIdentity(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"[HUB] User joined: {userId} (ConnectionId: {Context.ConnectionId})");
            return Guid.NewGuid().ToString();
        }

        // 2. Verifiering
        public async Task<bool> VerifyIdentity(string userId, string signature)
        {
            Console.WriteLine($"[HUB] User verified: {userId}");
            return true;
        }

        // 3. Skicka meddelande + Hantera Timer
        public async Task SendEnvelope(EnvelopeDto env)
        {
            Console.WriteLine($"[HUB] Message from {env.FromUserId} to {env.ToUserId}. Burn: {env.BurnAfterSeconds}s");

            // A. Skicka meddelandet till mottagaren direkt
            await Clients.Group(env.ToUserId).SendAsync("ReceiveEnvelope", env);

            // B. Om det finns en timer: Vänta och radera sedan för ALLA
            if (env.BurnAfterSeconds.HasValue && env.BurnAfterSeconds.Value > 0)
            {
                // Vi startar en bakgrundsprocess som inte blockerar resten av servern
                _ = Task.Delay(env.BurnAfterSeconds.Value * 1000).ContinueWith(async _ =>
                {
                    try 
                    {
                        // Skicka "DeleteEnvelope" till mottagaren
                        await Clients.Group(env.ToUserId).SendAsync("DeleteEnvelope", env.EnvelopeId);
                        
                        // Skicka "DeleteEnvelope" till avsändaren (så det försvinner för dig också)
                        await Clients.Group(env.FromUserId).SendAsync("DeleteEnvelope", env.EnvelopeId);
                        
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

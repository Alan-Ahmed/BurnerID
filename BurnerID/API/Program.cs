using API.Adapters;
using API.Extensions;
using BurnerBackend.Hubs;
using API.Hubs.Filters;
using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// --- 1. API EXPLORER ---
builder.Services.AddEndpointsApiExplorer();

// --- 2. CORS (DÖRRVAKTEN) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNetlify",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173", 
                    "https://luxury-bublanina-733452.netlify.app",
                    "https://burnerid-production.up.railway.app" // Tillåt även Railway-domänen själv
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // KRITISKT FÖR SIGNALR
        });
});

// --- 3. SIGNALR ---
builder.Services.AddSignalR();

// Registrera tjänster
builder.Services.AddSingleton<RequireAuthenticatedFilter>();
builder.Services.AddSingleton<RateLimitSendEnvelopeFilter>();
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddSingleton<IEnvelopeRouter, SignalREnvelopeRouter>();

var app = builder.Build();

// --- 4. MIDDLEWARE PIPELINE ---
app.UseAppMiddleware();
app.UseRouting();

// Aktivera CORS med rätt namn
app.UseCors("AllowNetlify");

// --- 5. ENDPOINTS ---
app.MapHub<BurnHub>("/burn");

app.Run();

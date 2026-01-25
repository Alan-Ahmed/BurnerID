using API.Adapters;
using API.Extensions;
// VIKTIGT: Se till att denna matchar namnet (namespace) i din BurnHub.cs-fil!
using BurnerBackend.Hubs;
using API.Hubs.Filters;
using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// --- 1. API EXPLORER (Behövs för minimal APIs, Swagger borttagen) ---
builder.Services.AddEndpointsApiExplorer();

// --- 2. CORS (DÖRRVAKTEN) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            // Tillåter både lokal utveckling och din skarpa Netlify-sida
            policy.WithOrigins(
                    "http://localhost:5173", 
                    "https://luxury-bublanina-733452.netlify.app"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // <--- KRITISKT FÖR SIGNALR
        });
});

// --- 3. SIGNALR & FILTER ---
builder.Services.AddSignalR(options =>
{
    // Filtren är tillfälligt inaktiverade enligt din kommentar för att undvika "Unauthorized"
    // options.AddFilter<RequireAuthenticatedFilter>();
    // options.AddFilter<RateLimitSendEnvelopeFilter>();
});

// Registrera filtren i DI-containern
builder.Services.AddSingleton<RequireAuthenticatedFilter>();
builder.Services.AddSingleton<RateLimitSendEnvelopeFilter>();

// --- 4. APP SERVICES ---
builder.Services.AddAppServices(builder.Configuration);

// Routingen för kuvert
builder.Services.AddSingleton<IEnvelopeRouter, SignalREnvelopeRouter>();

var app = builder.Build();

// --- 5. MIDDLEWARE PIPELINE ---

// Custom middleware
app.UseAppMiddleware();

app.UseRouting();

// --- 6. AKTIVERA CORS ---
// Måste ligga mellan UseRouting och MapHub
app.UseCors("AllowAll");

// --- 7. ENDPOINTS ---
// Vi kopplar "/burn" till BurnHub.
app.MapHub<BurnHub>("/burn");

app.Run();

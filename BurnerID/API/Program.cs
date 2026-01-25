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
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173", 
                    "https://luxury-bublanina-733452.netlify.app" // Din Netlify-frontend
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});

// --- 3. SIGNALR ---
builder.Services.AddSignalR();

// Registrera filter i DI
builder.Services.AddSingleton<RequireAuthenticatedFilter>();
builder.Services.AddSingleton<RateLimitSendEnvelopeFilter>();

// --- 4. APP SERVICES ---
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddSingleton<IEnvelopeRouter, SignalREnvelopeRouter>();

var app = builder.Build();

// --- 5. MIDDLEWARE ---
app.UseAppMiddleware();
app.UseRouting();

// Aktivera CORS innan Hubs
app.UseCors("AllowAll");

// --- 6. ENDPOINTS ---
app.MapHub<BurnHub>("/burn");

// Denna rad säkerställer att Azure kan binda till rätt port
app.Run();

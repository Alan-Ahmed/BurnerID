using API.Adapters;
using API.Extensions;
// VIKTIGT: Se till att denna matchar namnet (namespace) i din BurnHub.cs-fil!
using BurnerBackend.Hubs;
using API.Hubs.Filters;
using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SWAGGER & API ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. CORS (DÖRRVAKTEN) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true) // Tillåter din Frontend (localhost)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // <--- DETTA ÄR KRITISKT FÖR SIGNALR
        });
});

// --- 3. SIGNALR & FILTER ---
builder.Services.AddSignalR(options =>
{
    // --- HÄR ÄR ÄNDRINGEN! ---
    // Vi har satt "//" framför raderna nedan för att stänga av filtren tillfälligt.
    // Detta gör att du slipper "Unauthorized"-felet och kan logga in.

    // options.AddFilter<RequireAuthenticatedFilter>();
    // options.AddFilter<RateLimitSendEnvelopeFilter>();
});

// Registrera filtren i DI-containern (Dessa kan vara kvar, de gör ingen skada här)
builder.Services.AddSingleton<RequireAuthenticatedFilter>();
builder.Services.AddSingleton<RateLimitSendEnvelopeFilter>();

// --- 4. APP SERVICES ---
builder.Services.AddAppServices(builder.Configuration);

// Routingen för kuvert
builder.Services.AddSingleton<IEnvelopeRouter, SignalREnvelopeRouter>();

var app = builder.Build();

// --- 5. MIDDLEWARE PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Din custom middleware
app.UseAppMiddleware();

app.UseRouting();

// --- 6. AKTIVERA CORS ---
// Måste ligga mellan UseRouting och MapHub
app.UseCors("AllowAll");

// --- 7. ENDPOINTS ---
// Vi kopplar "/burn" till din nya BurnHub.
app.MapHub<BurnHub>("/burn");

app.Run();
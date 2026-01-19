using API.Adapters;
using API.Extensions;
using API.Hubs;
using API.Hubs.Filters;
using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SWAGGER & API ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. CORS (DÖRRVAKTEN) ---
// Vi lägger in konfigurationen direkt här så vi vet att den fungerar.
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
// Vi lägger till SignalR och registrerar dina filter
builder.Services.AddSignalR(options =>
{
    // Registrerar filtren globalt för hubben
    options.AddFilter<RequireAuthenticatedFilter>();
    options.AddFilter<RateLimitSendEnvelopeFilter>();
});

// Registrera filtren i DI-containern
builder.Services.AddSingleton<RequireAuthenticatedFilter>();
builder.Services.AddSingleton<RateLimitSendEnvelopeFilter>();

// --- 4. APP SERVICES ---
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddSingleton<IEnvelopeRouter, SignalREnvelopeRouter>();

var app = builder.Build();

// --- 5. MIDDLEWARE PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Om du har anpassad middleware i denna extension, kör den här.
// Om den krockar med något, prova att kommentera ut den tillfälligt.
app.UseAppMiddleware();

app.UseRouting();

// --- 6. AKTIVERA CORS ---
// Detta måste ligga mellan UseRouting och MapHub
app.UseCors("AllowAll");

// --- 7. ENDPOINTS ---
// OBS: Vi använder "/burn" för att matcha Frontend-prompten!
app.MapHub<ChatHub>("/burn");

app.Run();
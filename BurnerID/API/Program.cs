using API.Adapters;
using API.Extensions;
using API.Hubs;
using API.Hubs.Filters;
using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// 1. LÄGG TILL DESSA FÖR ATT AKTIVERA SWAGGER-TJÄNSTEN
builder.Services.AddEndpointsApiExplorer(); // <---
builder.Services.AddSwaggerGen();           // <---

builder.Services.AddLocalDevCors(builder.Configuration);
builder.Services.AddSignalRWithFilters();

// Hub filters
builder.Services.AddSingleton<RequireAuthenticatedFilter>();
builder.Services.AddSingleton<RateLimitSendEnvelopeFilter>();
builder.Services.AddSingleton<IHubFilter>(sp => sp.GetRequiredService<RequireAuthenticatedFilter>());
builder.Services.AddSingleton<IHubFilter>(sp => sp.GetRequiredService<RateLimitSendEnvelopeFilter>());

builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddSingleton<IEnvelopeRouter, SignalREnvelopeRouter>();

var app = builder.Build();

// 2. LÄGG TILL DETTA FÖR ATT VISA SIDAN I UTVECKLINGSLÄGE
if (app.Environment.IsDevelopment()) // <---
{
    app.UseSwagger();   // <--- Genererar JSON-filen
    app.UseSwaggerUI(); // <--- Genererar den grafiska webbsidan
}

app.UseAppMiddleware();
app.UseRouting();
app.UseLocalDevCors();

app.MapHub<ChatHub>("/chat");

app.Run();
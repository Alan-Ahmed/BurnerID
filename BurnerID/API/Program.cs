using API.Adapters;
using API.Extensions;
using API.Hubs;
using API.Hubs.Filters;
using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalDevCors(builder.Configuration);
builder.Services.AddSignalRWithFilters();

// Hub filters
builder.Services.AddSingleton<RequireAuthenticatedFilter>();
builder.Services.AddSingleton<RateLimitSendEnvelopeFilter>();
builder.Services.AddSingleton<IHubFilter>(sp => sp.GetRequiredService<RequireAuthenticatedFilter>());
builder.Services.AddSingleton<IHubFilter>(sp => sp.GetRequiredService<RateLimitSendEnvelopeFilter>());

builder.Services.AddAppServices(builder.Configuration);

// Bind router implementation (API adapter)
builder.Services.AddSingleton<IEnvelopeRouter, SignalREnvelopeRouter>();

var app = builder.Build();

app.UseAppMiddleware();
app.UseRouting();
app.UseLocalDevCors();

app.MapHub<ChatHub>("/chat");

app.Run();

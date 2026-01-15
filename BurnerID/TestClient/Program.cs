using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json; // Behövs för att läsa svaret

var url = "https://localhost:7008/chat";
Console.WriteLine($"Ansluter till {url}...");

var connection = new HubConnectionBuilder()
    .WithUrl(url)
    .Build();

try
{
    // --- STEG 0: ANSLUT ---
    await connection.StartAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("CONNECTED! Handskakning klar.");
    Console.ResetColor();

    // --- STEG 1: REQUEST CHALLENGE ---
    Console.WriteLine("\n[1] Skickar 'RequestChallenge'...");

    var userId = "TestUser_" + Guid.NewGuid().ToString().Substring(0, 8);
    var requestData = new { UserId = userId };

    // Vi tar emot svaret som ett generiskt objekt (JsonElement)
    var challengeResult = await connection.InvokeAsync<JsonElement>("RequestChallenge", requestData);

    // Skriv ut rådata
    Console.WriteLine($"SVAR: {challengeResult}");

    // Plocka ut värdena vi behöver för nästa steg
    string challengeId = challengeResult.GetProperty("challengeId").GetString()!;
    string nonce = challengeResult.GetProperty("nonceBase64Url").GetString()!;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"--> Fick ChallengeId: {challengeId}");
    Console.WriteLine($"--> Fick Nonce: {nonce}");
    Console.ResetColor();

    // --- STEG 2: AUTHENTICATE (Testar kopplingen) ---
    Console.WriteLine("\n[2] Försöker autentisera (Skickar fejk-signatur)...");

    // I en riktig app måste vi signera 'nonce' med Ed25519 här.
    // Nu skickar vi bara "dummy"-data för att se att metoden svarar.
    var authData = new
    {
        UserId = userId,
        ChallengeId = challengeId,
        PublicKeyBase64Url = "DUMMY_KEY_AAAA", // Servern kommer klaga på formatet
        SignatureBase64Url = "DUMMY_SIG_BBBB"  // Servern kommer se att denna är fel
    };

    try
    {
        var authResponse = await connection.InvokeAsync<object>("Authenticate", authData);
        Console.WriteLine("JÖSSES! Det lyckades (vilket det inte borde med fejk-data).");
    }
    catch (Exception ex)
    {
        // Vi FÖRVÄNTAR oss ett fel här eftersom vi skickar fejk-data.
        // Om felet är "invalid_signature" eller liknande, så fungerar servern perfekt!
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"SVAR FRÅN SERVER: {ex.Message}");
        Console.WriteLine("(Detta är bra! Servern avvisade vår falska inloggning.)");
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"KRITISKT FEL: {ex.Message}");
}

Console.ResetColor();
Console.WriteLine("\nTryck på valfri tangent för att avsluta...");
Console.ReadKey();
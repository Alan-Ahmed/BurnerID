FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Kopiera hela lösningen (inkl. BurnerID/API, BurnerID/Domain osv.)
COPY . .

# 2. Återställ beroenden genom att peka direkt på API-projektet
# (Kontrollera att sökvägen till din .csproj-fil är rätt nedan)
RUN dotnet restore "BurnerID/API/API.csproj"

# 3. Bygg och publicera
RUN dotnet publish "BurnerID/API/API.csproj" -c Release -o /app/out

# 4. Skapa kör-miljön
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Kontrollera om din fil heter API.dll eller BurnerID.API.dll
ENTRYPOINT ["dotnet", "API.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Kopiera ALLA filer så att bygget ser både API, Application och Infrastructure
COPY . .

# 2. Återställ beroenden för hela projektet
RUN dotnet restore "BurnerID/API/API.csproj"

# 3. Bygg och publicera API-projektet
RUN dotnet publish "BurnerID/API/API.csproj" -c Release -o /app/out

# 4. Skapa kör-miljön
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Railway standard-portar
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Starta API:et (Kontrollera att filnamnet är API.dll)
ENTRYPOINT ["dotnet", "API.dll"]

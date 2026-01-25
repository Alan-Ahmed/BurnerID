FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiera API-projektet (vi är nu inuti API-mappen)
COPY *.csproj ./
RUN dotnet restore

# Kopiera resten av koden i API-mappen och bygg
COPY . ./
RUN dotnet publish -c Release -o /app

# Skapa kör-miljön
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Kontrollera om din fil heter API.dll eller BurnerID.API.dll
# Om den heter något annat, ändra namnet här under:
ENTRYPOINT ["dotnet", "API.dll"]

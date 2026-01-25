FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopiera projektfiler och återställ
COPY *.csproj ./
RUN dotnet restore

# Kopiera allt annat och bygg projektet
COPY . ./
RUN dotnet publish -c Release -o out

# Skapa själva kör-miljön
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Railway kräver ofta att vi lyssnar på alla adresser
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# VIKTIGT: Byt ut BurnerID.dll om ditt projekt heter något annat i VS
ENTRYPOINT ["dotnet", "BurnerID.dll"]

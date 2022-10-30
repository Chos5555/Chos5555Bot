# Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Chos5555Bot/Chos5555Bot.csproj", "Chos5555Bot/"]

RUN dotnet restore "Chos5555Bot/Chos5555Bot.csproj"

COPY . .

WORKDIR "/src/Chos5555Bot"

RUN dotnet build "Chos5555Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chos5555Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chos5555Bot.dll"]
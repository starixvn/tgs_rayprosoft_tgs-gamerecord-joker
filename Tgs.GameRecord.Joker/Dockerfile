#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.Joker/Tgs.GameRecord.Joker.csproj", "Tgs.GameRecord.Joker/"]
COPY ["Tgs.GameRecord.Joker.Core/Tgs.GameRecord.Joker.Core.csproj", "Tgs.GameRecord.Joker.Core/"]
COPY ["Tgs.GameRecord.Joker.Common/Tgs.GameRecord.Joker.Common.csproj", "Tgs.GameRecord.Joker.Common/"]
RUN dotnet restore "./Tgs.GameRecord.Joker/Tgs.GameRecord.Joker.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.Joker"
RUN dotnet build "./Tgs.GameRecord.Joker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.Joker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.Joker.dll"]
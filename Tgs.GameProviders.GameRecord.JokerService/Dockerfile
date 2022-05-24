#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameProviders.GameRecord.JokerService/Tgs.GameProviders.GameRecord.JokerService.csproj", "Tgs.GameProviders.GameRecord.JokerService/"]
COPY ["Tgs.GameProviders.GameRecord.JokerService.Core/Tgs.GameProviders.GameRecord.JokerService.Core.csproj", "Tgs.GameProviders.GameRecord.JokerService.Core/"]
RUN dotnet restore "Tgs.GameProviders.GameRecord.JokerService/Tgs.GameProviders.GameRecord.JokerService.csproj"
COPY . .
WORKDIR "/src/Tgs.GameProviders.GameRecord.JokerService"
RUN dotnet build "Tgs.GameProviders.GameRecord.JokerService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameProviders.GameRecord.JokerService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameProviders.GameRecord.JokerService.dll"]
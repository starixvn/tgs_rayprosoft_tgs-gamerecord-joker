#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.JokerService/Tgs.GameRecord.JokerService.csproj", "Tgs.GameRecord.JokerService/"]
COPY ["Tgs.GameRecord.JokerService.Core/Tgs.GameRecord.JokerService.Core.csproj", "Tgs.GameRecord.JokerService.Core/"]
RUN dotnet restore "Tgs.GameRecord.JokerService/Tgs.GameRecord.JokerService.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.JokerService"
RUN dotnet build "Tgs.GameRecord.JokerService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameRecord.JokerService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.JokerService.dll"]
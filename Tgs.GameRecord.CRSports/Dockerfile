#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Tgs.GameRecord.CRSports/Tgs.GameRecord.CRSports.csproj", "Tgs.GameRecord.CRSports/"]
COPY ["Tgs.GameRecord.CRSports.Common/Tgs.GameRecord.CRSports.Common.csproj", "Tgs.GameRecord.CRSports.Common/"]
COPY ["Tgs.GameRecord.CRSports.Core/Tgs.GameRecord.CRSports.Core.csproj", "Tgs.GameRecord.CRSports.Core/"]
COPY ["Tgs.GameRecord.CRSports.Repositories/Tgs.GameRecord.CRSports.Repositories.csproj", "Tgs.GameRecord.CRSports.Repositories/"]
RUN dotnet restore "./Tgs.GameRecord.CRSports/Tgs.GameRecord.CRSports.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.CRSports"
RUN dotnet build "./Tgs.GameRecord.CRSports.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.CRSports.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.CRSports.dll"]
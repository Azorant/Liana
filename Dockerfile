FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Liana.sln", "."]
COPY ["Liana.Startup/Liana.Startup.csproj", "Liana.Startup/"]
COPY ["Liana.API/Liana.API.csproj", "Liana.API/"]
COPY ["Liana.Bot/Liana.Bot.csproj", "Liana.Bot/"]
COPY ["Liana.Database/Liana.Database.csproj", "Liana.Database/"]
COPY ["Liana.Models/Liana.Models.csproj", "Liana.Models/"]
RUN dotnet restore
COPY . .
WORKDIR "/src/Liana.Startup"
RUN dotnet build "Liana.Startup.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Liana.Startup.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5123
ENTRYPOINT ["dotnet", "Liana.Startup.dll"]
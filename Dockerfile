FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bot.Template/Bot.Template.csproj", "Bot.Template/"]
RUN dotnet restore "Bot.Template/Bot.Template.csproj"
COPY . .
WORKDIR "/src/Bot.Template"
RUN dotnet build "Bot.Template.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bot.Template.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.Template.dll"]

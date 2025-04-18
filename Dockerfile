# Use the appropriate Windows-based images
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN mkdir uploads
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BusinessNewEnvironment/BusinessNewEnvironment.csproj", "BusinessNewEnvironment/"]
RUN dotnet restore "BusinessNewEnvironment/BusinessNewEnvironment.csproj"
COPY . .
WORKDIR "/src/BusinessNewEnvironment"
RUN dotnet build "BusinessNewEnvironment.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BusinessNewEnvironment.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BusinessNewEnvironment.dll"]
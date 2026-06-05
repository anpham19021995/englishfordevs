# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY EnglishForDevs.Api.csproj ./
RUN dotnet restore ./EnglishForDevs.Api.csproj
COPY . ./
RUN dotnet publish ./EnglishForDevs.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "EnglishForDevs.Api.dll"]

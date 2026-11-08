#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet restore "SudokuCollective.Api/SudokuCollective.Api.csproj"

FROM build AS publish
RUN dotnet publish "SudokuCollective.Api/SudokuCollective.Api.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["sh", "-c", "dotnet SudokuCollective.Api.dll --urls http://*:${PORT:-8080}"]

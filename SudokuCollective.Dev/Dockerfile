#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SudokuCollective.Dev/SudokuCollective.Dev.csproj", "SudokuCollective.Dev/"]
COPY ["SudokuCollective.Core/SudokuCollective.Core.csproj", "SudokuCollective.Core/"]
COPY ["SudokuCollective.Data/SudokuCollective.Data.csproj", "SudokuCollective.Data/"]
COPY ["SudokuCollective.Encrypt/SudokuCollective.Encrypt.csproj", "SudokuCollective.Encrypt/"]
COPY ["SudokuCollective.Logs/SudokuCollective.Logs.csproj", "SudokuCollective.Logs/"]
RUN dotnet restore "SudokuCollective.Dev/SudokuCollective.Dev.csproj"
COPY . .
WORKDIR "/src/SudokuCollective.Dev"
RUN dotnet build "SudokuCollective.Dev.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SudokuCollective.Dev.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SudokuCollective.Dev.dll"]
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["AutoSats/AutoSats.csproj", "AutoSats/"]
RUN dotnet restore "AutoSats/AutoSats.csproj"
COPY . .
WORKDIR "/src/AutoSats"
RUN dotnet build "AutoSats.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoSats.csproj" -c Release -o /app/publish

FROM base AS final
ENV ConnectionStrings:AutoSatsDatabase="Data Source=/app_data/AutoSats.db"
RUN mkdir /app_data
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoSats.dll"]
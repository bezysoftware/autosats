FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80

# restore solution packages
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS restore
WORKDIR /src
COPY ["AutoSats/AutoSats.csproj", "AutoSats/"]
COPY ["AutoSats.Tests/AutoSats.Tests.csproj", "AutoSats.Tests/"]
COPY ["AutoSats.sln", ""]
RUN dotnet restore

# build the main project
FROM restore AS build
COPY . .
WORKDIR "/src/AutoSats"
RUN dotnet build -c Release -o /app/build

# run tests
FROM build AS test
WORKDIR "/src/AutoSats.Tests"
RUN dotnet test -c Release -o /app/build

# publish
FROM test as publish
WORKDIR "/src/AutoSats"
RUN dotnet publish "AutoSats.csproj" -c Release -o /app/publish

# final
FROM base AS final
ENV ConnectionStrings__AutoSatsDatabase="Data Source=/app_data/AutoSats.db"
RUN mkdir /app_data
VOLUME /app_data
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoSats.dll"]
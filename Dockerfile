FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY src/AgencyQueue.Api/AgencyQueue.Api.csproj src/AgencyQueue.Api/

RUN dotnet restore src/AgencyQueue.Api/AgencyQueue.Api.csproj

COPY src/AgencyQueue.Api/ src/AgencyQueue.Api/

RUN dotnet publish src/AgencyQueue.Api/AgencyQueue.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "AgencyQueue.Api.dll"]

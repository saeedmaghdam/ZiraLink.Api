#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app
EXPOSE 6000
EXPOSE 6001

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS restore
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["./src/ZiraLink.Api/ZiraLink.Api.csproj", "./src/ZiraLink.Api/"]
RUN dotnet restore "./src/ZiraLink.Api/ZiraLink.Api.csproj"

FROM restore AS build
WORKDIR /src
COPY ./src/ ./src/
WORKDIR "./src/ZiraLink.Api"
RUN dotnet build "ZiraLink.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZiraLink.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y ca-certificates grep netcat-openbsd iputils-ping iproute2

COPY ./certs/ /usr/local/share/ca-certificates/
RUN update-ca-certificates

ENTRYPOINT ["dotnet", "ZiraLink.Api.dll"]

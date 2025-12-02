FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Zoologico.API/Zoologico.API.csproj", "Zoologico.API/"]
RUN dotnet restore "Zoologico.API/Zoologico.API.csproj"
COPY . .


WORKDIR "/src/Zoologico.API"

RUN dotnet build "Zoologico.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Zoologico.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zoologico.API.dll"]
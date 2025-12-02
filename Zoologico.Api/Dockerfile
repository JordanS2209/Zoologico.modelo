FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Zoologico.modelo.csproj", "./"]
RUN dotnet restore "Zoologico.modelo.csproj"
COPY . .
RUN dotnet build "Zoologico.modelo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Zoologico.modelo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zoologico.modelo.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Zoologico.Api/Zoologico.Api.csproj", "Zoologico.Api/"]
RUN dotnet restore "Zoologico.Api/Zoologico.Api.csproj"
COPY . .


WORKDIR "/src/Zoologico.Api"

RUN dotnet build "Zoologico.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Zoologico.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zoologico.Api.dll"]
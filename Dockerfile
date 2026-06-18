# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["Sakrus/Sakrus.csproj", "Sakrus/"]
COPY ["Sakrus.Core/Sakrus.Core.csproj", "Sakrus.Core/"]
COPY ["Sakrus.Infrastructure/Sakrus.Infrastructure.csproj", "Sakrus.Infrastructure/"]
RUN dotnet restore "Sakrus/Sakrus.csproj"

# Copiar o restante do código e compilar
COPY . .
WORKDIR "/src/Sakrus"
RUN dotnet build "Sakrus.csproj" -c Release -o /app/build

# Estágio 2: Publish
FROM build AS publish
RUN dotnet publish "Sakrus.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Runtime Final (Produção)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# ROB-06: Healthcheck configurado
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
  CMD curl --fail http://localhost:8080/login || exit 1

ENTRYPOINT ["dotnet", "Sakrus.dll"]
# ---- Estágio de build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0.422 AS build
WORKDIR /src

COPY ["src/UsuariosAPI.API/UsuariosAPI.API.csproj", "src/UsuariosAPI.API/"]
COPY ["src/UsuariosAPI.Application/UsuariosAPI.Application.csproj", "src/UsuariosAPI.Application/"]
COPY ["src/UsuariosAPI.Domain/UsuariosAPI.Domain.csproj", "src/UsuariosAPI.Domain/"]
COPY ["src/UsuariosAPI.Infrastructure/UsuariosAPI.Infrastructure.csproj", "src/UsuariosAPI.Infrastructure/"]
RUN dotnet restore "src/UsuariosAPI.API/UsuariosAPI.API.csproj"

COPY . .
WORKDIR "/src/src/UsuariosAPI.API"
RUN dotnet build "UsuariosAPI.API.csproj" -c Release -o /app/build

# ---- Estágio de publicação ----
FROM build AS publish
RUN dotnet publish "UsuariosAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Imagem final ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0.28 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# O usuário não privilegiado já é fornecido pelas imagens oficiais do .NET 8.
USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD bash -c "exec 3<>/dev/tcp/127.0.0.1/8080 && printf 'GET /health HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n' >&3 && grep -q '200 OK' <&3"

ENTRYPOINT ["dotnet", "UsuariosAPI.API.dll"]

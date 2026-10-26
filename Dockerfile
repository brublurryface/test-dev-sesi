# O frontend e a API são compilados em etapas separadas.
# A imagem final contém somente o runtime e a aplicação publicada.
FROM node:24.19.0-bookworm-slim AS frontend
WORKDIR /src/frontend

# Instalar antes de copiar as telas permite reaproveitar a camada de dependências.
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0.402 AS api
WORKDIR /src
COPY global.json Directory.Build.props ./
COPY backend/SesiIndustria.Api/SesiIndustria.Api.csproj backend/SesiIndustria.Api/packages.lock.json ./backend/SesiIndustria.Api/
RUN dotnet restore backend/SesiIndustria.Api/SesiIndustria.Api.csproj --locked-mode
COPY backend/SesiIndustria.Api/ ./backend/SesiIndustria.Api/
RUN dotnet publish backend/SesiIndustria.Api/SesiIndustria.Api.csproj -c Release --no-restore -p:UseAppHost=false -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0.13 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=api /out/ ./
COPY --from=frontend /src/frontend/dist/test-dev-sesi-industria/browser/ ./wwwroot/

# O usuário sem privilégios precisa escrever somente o banco demonstrativo.
RUN mkdir -p /app/App_Data && chown 1654:1654 /app/App_Data
USER 1654:1654
EXPOSE 10000

# Render fornece PORT; o valor padrão também permite executar a imagem localmente.
# exec entrega os sinais de encerramento diretamente ao processo .NET.
CMD ["/bin/sh", "-c", "exec dotnet SesiIndustria.Api.dll --urls \"http://0.0.0.0:${PORT:-10000}\""]

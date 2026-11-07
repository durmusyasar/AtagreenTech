# syntax=docker/dockerfile:1
#
# Stok servisi imajı. Build context repo köküdür:
#
#   GITHUB_PACKAGES_TOKEN=$(gh auth token) docker build \
#     --secret id=github_packages_token,env=GITHUB_PACKAGES_TOKEN \
#     --build-arg GITHUB_USERNAME=<kullanıcı> \
#     -f src/Services/StockService/AtaGreenTech.StockService.Api/Dockerfile .
#
# Aynı imaj API'yi (varsayılan), `migrate-master` ve `migrate-tenants` komutlarını ve container sağlık kontrolünü
# (`healthcheck`) çalıştırır.

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ENV DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /src

# Restore katmanı yalnızca proje ve paket tanımları değiştiğinde yeniden oluşturulur.
COPY nuget.config ./
COPY src/Shared/SharedKernel/AtaGreenTech.SharedKernel.csproj src/Shared/SharedKernel/
COPY src/Services/StockService/AtaGreenTech.StockService.Domain/AtaGreenTech.StockService.Domain.csproj src/Services/StockService/AtaGreenTech.StockService.Domain/
COPY src/Services/StockService/AtaGreenTech.StockService.Application/AtaGreenTech.StockService.Application.csproj src/Services/StockService/AtaGreenTech.StockService.Application/
COPY src/Services/StockService/AtaGreenTech.StockService.Infrastructure/AtaGreenTech.StockService.Infrastructure.csproj src/Services/StockService/AtaGreenTech.StockService.Infrastructure/
COPY src/Services/StockService/AtaGreenTech.StockService.Api/AtaGreenTech.StockService.Api.csproj src/Services/StockService/AtaGreenTech.StockService.Api/

# OmniSync.Core paketleri GitHub Packages'tan gelir. Token yalnızca bu komut süresince bağlanan bir BuildKit
# secret'ıdır; imaj katmanlarına ve geçmişine yazılmaz.
ARG GITHUB_USERNAME
RUN --mount=type=secret,id=github_packages_token,env=GITHUB_PACKAGES_TOKEN,required=true \
    GITHUB_USERNAME="${GITHUB_USERNAME:?GITHUB_USERNAME build argümanı gerekli}" \
    dotnet restore src/Services/StockService/AtaGreenTech.StockService.Api/AtaGreenTech.StockService.Api.csproj

COPY src/ src/
RUN dotnet publish src/Services/StockService/AtaGreenTech.StockService.Api/AtaGreenTech.StockService.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

# Chiseled imajlarda shell ve paket yöneticisi yoktur; varsayılan kullanıcı root olmayan "app"tır (UID 1654).
# "extra" varyantı kültür ve saat dilimi verilerini (ICU, tzdata) içerir. Uygulama dosyaları root'a ait kalır,
# uygulama kullanıcısı bunları değiştiremez.
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080

# dotnet doğrudan (exec formunda) çalıştırılır; SIGTERM shell'e değil uygulamaya ulaşır ve uygulama devam eden
# istekleri bitirerek kapanır.
ENTRYPOINT ["dotnet", "AtaGreenTech.StockService.Api.dll"]

HEALTHCHECK --interval=15s --timeout=10s --start-period=20s --retries=3 \
    CMD ["dotnet", "AtaGreenTech.StockService.Api.dll", "healthcheck"]

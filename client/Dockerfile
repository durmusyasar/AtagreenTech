# syntax=docker/dockerfile:1

FROM node:22-alpine AS build
WORKDIR /src

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci --no-audit --no-fund

COPY . .
RUN npm run lint && npm test && npm run build

# Root olmayan kullanıcıyla (uid 101) 8080'den dinleyen nginx.
FROM nginxinc/nginx-unprivileged:1.30-alpine

# Şablon yalnızca ATAGREEN_* değişkenleri ve entrypoint'in /etc/resolv.conf'tan ürettiği DNS adresiyle işlenir.
ENV NGINX_ENTRYPOINT_LOCAL_RESOLVERS=1 \
    NGINX_ENVSUBST_FILTER="^(ATAGREEN_|NGINX_LOCAL_RESOLVERS)" \
    ATAGREEN_STOCK_SERVICE_UPSTREAM=stock-service:8080 \
    ATAGREEN_SERVER_NAMES=".omnisync.com .omnisync.com.tr .omnisync.localhost" \
    ATAGREEN_TRUSTED_PROXY=127.0.0.1

COPY nginx/security-headers.conf /etc/nginx/security-headers.conf
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /src/dist /usr/share/nginx/html

EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -q --spider http://127.0.0.1:8080/healthz || exit 1

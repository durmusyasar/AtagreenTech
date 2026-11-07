#!/usr/bin/env bash
# docker-compose.yml'nin beklediği secret dosyalarını rastgele değerlerle oluşturur. Var olan dosyalara dokunmaz;
# böylece tekrar çalıştırmak çalışan bir kurulumun parolalarını değiştirmez.
#
#   deploy/generate-secrets.sh [secrets-dizini]

set -euo pipefail

dir="${1:-secrets}"
mkdir -p "$dir"
chmod 700 "$dir"
umask 077

write_secret() {
  local name="$1" value="$2"
  if [[ -e "$dir/$name" ]]; then
    echo "skipped $dir/$name (already exists)"
    return
  fi
  printf '%s' "$value" > "$dir/$name"
  echo "created $dir/$name"
}

write_secret postgres_password "$(openssl rand -hex 32)"
write_secret jwt_signing_key "$(openssl rand -base64 48 | tr -d '\n')"

# Bağlantı dizesi, veritabanı parolasıyla aynı kalmalıdır. Chiseled imajda Kerberos kütüphanesi bulunmadığından
# GSS şifrelemesi kapatılır (tenant bağlantı dizelerine de eklenmelidir).
write_secret master_connection_string \
  "Host=postgres;Database=atagreen_stock_master;Username=atagreen;Password=$(cat "$dir/postgres_password");GSS Encryption Mode=Disable"

cat <<EOF

Linux'ta stok servisi ve migrator container'ları UID 1654 ile çalışır; okudukları secret'ları bu kullanıcıya verin:
  sudo chown 1654:1654 $dir/master_connection_string $dir/jwt_signing_key
EOF

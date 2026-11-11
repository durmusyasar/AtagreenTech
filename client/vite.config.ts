/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react()],
    server: {
      port: 5173,
      strictPort: true,
      // Uygulama API'ye her ortamda göreli /api yoluyla gider; geliştirmede bu istekler yerel stok servisine
      // aktarılır, Docker'da aynı işi nginx yapar. Böylece CORS'a gerek kalmaz.
      proxy: {
        '/api': env.DEV_API_PROXY_TARGET || 'http://localhost:5154',
      },
    },
    build: {
      // Ant Design paketi tek başına ~860 kB (gzip ~280 kB); uyarı yalnızca bunu aşan büyümede tetiklenir.
      chunkSizeWarningLimit: 1000,
      rolldownOptions: {
        output: {
          // Sık değişen uygulama kodu, nadiren değişen kütüphanelerden ayrı paketlenir; sürüm güncellemelerinde
          // tarayıcı önbelleğindeki kütüphane paketleri geçerli kalır.
          codeSplitting: {
            groups: [
              { name: 'react', test: /node_modules[\\/](react|react-dom|react-router|react-router-dom|scheduler)[\\/]/ },
              { name: 'antd', test: /node_modules[\\/](antd|@ant-design|@rc-component|rc-[^\\/]+)[\\/]/ },
              { name: 'vendor', test: /node_modules[\\/]/ },
            ],
          },
        },
      },
    },
    test: {
      environment: 'node',
    },
  }
})

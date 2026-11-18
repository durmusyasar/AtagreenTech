/// <reference types="vitest/config" />
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

const rootDir = path.dirname(fileURLToPath(import.meta.url))

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const stockTarget = env.DEV_API_PROXY_TARGET || 'http://localhost:5154'
  const userTarget = env.DEV_USER_API_PROXY_TARGET || 'http://localhost:5155'

  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': path.join(rootDir, 'src'),
      },
    },
    server: {
      port: 5173,
      strictPort: true,
      // Uygulama API'ye her ortamda göreli /api yoluyla gider. Geliştirmede kimlik uçları UserService'e,
      // diğerleri StockService'e aktarılır (Docker'da aynı ayrımı nginx yapar).
      proxy: {
        '^/api/v1/(auth|users|roles|permissions|sessions|audit-logs|navigation)(/|$)': {
          target: userTarget,
          changeOrigin: true,
        },
        '/api': {
          target: stockTarget,
          changeOrigin: true,
        },
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

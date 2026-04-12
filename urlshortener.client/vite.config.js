import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 3000,
        host: true,
        proxy: {
            '/api': {
                target: 'http://localhost:5000',
                changeOrigin: true,
                secure: false,
                ws: true,
                configure: (proxy, _options) => {
                  proxy.on('error', (err, _req, _res) => {
                    console.log('proxy error', err);
                  });
                }
            }
        }
    }
})
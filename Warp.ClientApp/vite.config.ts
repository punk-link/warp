import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '')
    const origin = (process.env.ENV_ORIGIN || env.ENV_ORIGIN || 'https://localhost:8001').replace(/\/$/, '')

    return {
        plugins: [vue()],
        base: '/',
        cacheDir: '/tmp/vite-cache',
        define: {
            'import.meta.env.VITE_APP_VERSION': JSON.stringify(process.env.npm_package_version ?? '0.0.0')
        },
        server: {
            port: 5173,
            strictPort: true,
            proxy: {
                '/api': { 
                    target: origin, 
                    changeOrigin: true, 
                    secure: false 
                },
                '/config.js': { 
                    target: origin, 
                    changeOrigin: true, 
                    secure: false 
                },
                '/analytics.js': { 
                    target: origin, 
                    changeOrigin: true, 
                    secure: false 
                },
            }
        },
        build: {
            outDir: 'dist',
            emptyOutDir: true,
            sourcemap: true
        },
    }
})

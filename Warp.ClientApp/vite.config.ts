import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '')
    const origin = (env.ENV_ORIGIN || 'https://localhost:8001').replace(/\/$/, '')

    return {
        plugins: [vue()],
        base: '/',
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

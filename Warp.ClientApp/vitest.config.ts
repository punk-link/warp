import { fileURLToPath } from 'node:url'
import { mergeConfig } from 'vite'
import viteConfig from './vite.config'


const baseConfig = typeof viteConfig === 'function'
    ? viteConfig({ command: 'serve', mode: process.env.NODE_ENV ?? 'test' })
    : viteConfig


export default mergeConfig(baseConfig, {
    test: {
        globals: true,
        environment: 'jsdom',
        root: fileURLToPath(new URL('./', import.meta.url)),
        setupFiles: ['./vitest.setup.ts'],
        include: ['src/**/*.spec.ts', 'src/**/*.test.ts'],
        css: false,
        coverage: {
            provider: 'v8',
            reporter: ['text', 'html'],
            reportsDirectory: './coverage'
        }
    }
})

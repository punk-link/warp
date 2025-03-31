import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
    build: {
        outDir: 'wwwroot/dist',
        emptyOutDir: true,
        rollupOptions: {
            input: {
                'deleted': resolve(__dirname, 'wwwroot/js/features/deleted/index.js'),
                'error': resolve(__dirname, 'wwwroot/js/features/error/index.js'),
                'entry': resolve(__dirname, 'wwwroot/js/features/entry/index.js'),
                'main': resolve(__dirname, 'wwwroot/js/features/index/index.js'),
                'preview': resolve(__dirname, 'wwwroot/js/features/preview/index.js'),

                'fancybox': resolve(__dirname, 'node_modules/@fancyapps/ui/dist/fancybox/fancybox.css')
            },
            output: {
                entryFileNames: '[name].js',
                chunkFileNames: '[name]-[hash].js',
                assetFileNames: (assetInfo) => {
                    // Keep original CSS names for libraries we want to reference directly
                    if (assetInfo.name.includes('fancybox') && assetInfo.name.endsWith('.css')) 
                        return 'fancybox.css';
                    
                    // Default naming for other assets
                    return '[name].[ext]';
                }
            }
        }
    },
    resolve: {
        alias: {
            '/js': resolve(__dirname, 'wwwroot/js')
        }
    }
});
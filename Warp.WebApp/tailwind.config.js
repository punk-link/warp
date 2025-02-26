module.exports = {
    content: [
        "./**/*.{html,cshtml,js,jsx,ts,tsx}",
    ],
    theme: {
        screens: {
            'sm': '640px',
            'md': '768px',
            'lg': '1024px',
            'xl': '1280px',
            '2xl': '1536px',
        },
        extend: {
            zIndex: {
                '-10': '-10',
            },
        },
    },
    plugins: [],
}

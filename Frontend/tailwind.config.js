/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./src/**/*.{html,ts}",
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    50: '#fef7ec',
                    100: '#fdebcd',
                    200: '#fbd69b',
                    300: '#f9c169',
                    400: '#f7ab37',
                    500: '#f6ae40',
                    600: '#dd9c39',
                    700: '#b88230',
                    800: '#936826',
                    900: '#7a5720',
                    950: '#5c4118',
                },
                secondary: {
                    50: '#eef3fb',
                    100: '#dde7f7',
                    200: '#bacfef',
                    300: '#98b6e7',
                    400: '#5184da',
                    500: '#3167d1',
                    600: '#2c5cbc',
                    700: '#244d9d',
                    800: '#1d3d7e',
                    900: '#183367',
                    950: '#102245',
                },
                success: '#28a745',
                warning: '#ffc107',
                danger: '#dc3545',
                info: '#17a2b8',
                neutral: {
                    background: '#f8f9fa',
                    card: '#ffffff',
                    text: '#343a40'
                }
            },
            fontFamily: {
                sans: ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'Noto Sans', 'sans-serif'],
            },
            animation: {
                'fade-in': 'fadeIn 0.5s ease-out',
                'slide-up': 'slideUp 0.5s ease-out',
            },
            keyframes: {
                fadeIn: {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                slideUp: {
                    '0%': { transform: 'translateY(20px)', opacity: '0' },
                    '100%': { transform: 'translateY(0)', opacity: '1' },
                }
            }
        },
    },
    plugins: [],
}

/** @type {import('tailwindcss').Config} */
module.exports = {
    content: {
        files: [
            "index.html",
            "./src/**/*.rs",
        ],
    },
    theme: {
        extend: {
            borderRadius: {
                '3_xl': '1.5rem',
            }
        },
    },
}
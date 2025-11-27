/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./wwwroot/**/*.{js,html}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#0767B3',
          light: '#0e7fd1',
          dark: '#054f82',
        },
        secondary: {
          DEFAULT: '#0a8ed9',
          light: '#3ba6e6',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        heading: ['Poppins', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
}

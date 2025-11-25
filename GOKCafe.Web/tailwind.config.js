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
          DEFAULT: '#8B4513',
          light: '#A0522D',
          dark: '#654321',
        },
        secondary: {
          DEFAULT: '#D2691E',
          light: '#F4A460',
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

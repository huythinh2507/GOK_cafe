/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./wwwroot/**/*.{js,html,css}",
    "./wwwroot/js/**/*.js",
    "./node_modules/flowbite/**/*.js"
  ],

  theme: {
    // 👇 FIX: Override global breakpoints to remove default 1280px
    screens: {
      sm: '640px',
      md: '768px',
      lg: '1024px',
      xl: '1340px',  // Custom breakpoint replacing default 1280px
      '2xl': '1536px',
    },

    container: {
      center: true,
      padding: {
        DEFAULT: '1rem',
        sm: '0rem',
        lg: '0rem',
        xl: '0rem',
        '2xl': '0rem',
      },
    },

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
    cream: {
              50: '#FFFAF0',
              100: '#EBE5D8',
              200: '#D7CBB1',
              300: '#C3B18A',
              400: '#AF9763',
              500: '#9B7D3C',
              600: '#7C6430',
              700: '#5D4B24',
              800: '#3E3218',
              900: '#1F190C',
            },
      accentColor: {
        primary: '#0767B3',
      },

      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        heading: ['Poppins', 'system-ui', 'sans-serif'],
      },
    },
  },

  plugins: [
    require('flowbite/plugin')
  ],
}

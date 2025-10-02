/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/**/*.{html,ts}',
  ],
  theme: {
    extend: {
      fontFamily: {
        'sans': ['Poppins', 'system-ui', 'sans-serif'],
      },
      colors: {
        brand: {
          primary: '#22c55e',
          primaryDark: '#16a34a',
          primaryLight: '#6ee7b7',
          accent: '#3b82f6',
          accentDark: '#2563eb',
          accentLight: '#93c5fd',
          danger: '#ef4444',
          warning: '#f59e0b',
          info: '#0ea5e9',
          neutral: '#1f2937'
        }
      },
      boxShadow: {
        'soft-card': '0 8px 24px -4px rgba(0,0,0,0.08), 0 4px 12px -2px rgba(0,0,0,0.04)',
      },
      borderRadius: {
        'xl': '1rem',
      },
    },
  },
  plugins: [require('@tailwindcss/forms')],
}
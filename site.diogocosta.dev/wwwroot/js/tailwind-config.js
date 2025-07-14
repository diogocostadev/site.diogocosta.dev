// Configuração personalizada do Tailwind para tipografia técnica
tailwind.config = {
  theme: {
    extend: {
      fontFamily: {
        'primary': ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'system-ui', 'sans-serif'],
        'mono': ['JetBrains Mono', 'SF Mono', 'Monaco', 'Cascadia Code', 'Roboto Mono', 'Consolas', 'Courier New', 'monospace'],
        'display': ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'system-ui', 'sans-serif'],
        'sans': ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'system-ui', 'sans-serif']
      },
      fontSize: {
        'xs': ['0.75rem', { lineHeight: '1rem', letterSpacing: '0.025em' }],
        'sm': ['0.875rem', { lineHeight: '1.25rem', letterSpacing: '0.01em' }],
        'base': ['1rem', { lineHeight: '1.5rem', letterSpacing: '0em' }],
        'lg': ['1.125rem', { lineHeight: '1.75rem', letterSpacing: '-0.01em' }],
        'xl': ['1.25rem', { lineHeight: '1.75rem', letterSpacing: '-0.01em' }],
        '2xl': ['1.5rem', { lineHeight: '2rem', letterSpacing: '-0.015em' }],
        '3xl': ['1.875rem', { lineHeight: '2.25rem', letterSpacing: '-0.02em' }],
        '4xl': ['2.25rem', { lineHeight: '2.5rem', letterSpacing: '-0.025em' }],
        '5xl': ['3rem', { lineHeight: '1.1', letterSpacing: '-0.025em' }],
        '6xl': ['3.75rem', { lineHeight: '1.1', letterSpacing: '-0.03em' }],
      },
      colors: {
        'tech': {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
        }
      }
    }
  },
  plugins: []
}

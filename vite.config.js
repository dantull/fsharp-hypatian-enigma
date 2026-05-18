import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  root: "./src",
  build: {
    outDir: "../dist",
  },
  test: {
    globals: true, // enables afterEach `cleanup` from RTL. Without this all components will stay mounted after each test
    include: ['**/*.{test,spec}.?(c|m|fs.)[jt]s?(x)'],
    environment: 'jsdom',
    setupFiles: ['./vitest-setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      reportsDirectory: '../coverage',
      all: true,
      include: ['**/*.{fs,fsx,fs.jsx}'],
      exclude: ['**/*.test.*', 'fable_modules/**', 'node_modules/**', '**/bin/**', '**/obj/**'],
    },
  },
  server: {
      watch: {
          ignored: [ "**/*.fs" ]
      },
  }
})

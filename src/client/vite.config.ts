import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import path from 'path';

import fs from 'fs'

const configPath = path.resolve(__dirname, 'public/config.json');
const config = JSON.parse(fs.readFileSync(configPath, 'utf-8'));

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    host: '0.0.0.0',
    https: {
      key: fs.readFileSync(config.KEY_LOCATION),
      cert: fs.readFileSync(config.CERT_LOCATION),
    },
  }
})

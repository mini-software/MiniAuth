import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path';
const pathResolve = (dir) => {
  return resolve(__dirname, dir);
};
export default defineConfig({
  base: '/miniauth/', 
  plugins: [
    vue(),
  ],
  resolve: {
    alias: {
      '@': pathResolve('src')
    }
  }
})

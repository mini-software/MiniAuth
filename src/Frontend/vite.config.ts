import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import VueJsx from '@vitejs/plugin-vue-jsx'
import { resolve } from 'path';
const pathResolve = (dir) => {
  return resolve(__dirname, dir);
};
export default defineConfig({
  base: '/miniauth/', 
  build: {  
    emptyOutDir: true,
    outDir: '../MiniAuth/wwwroot/',
  },  
  plugins: [
    vue(),
    VueJsx(),
  ],
  resolve: {
    alias: {
      '@': pathResolve('src')
    }
  }
})

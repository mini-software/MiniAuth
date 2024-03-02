<script lang="ts" setup>
import { ref } from 'vue';
import BreadcrumbDefault from '@/components/Breadcrumbs/BreadcrumbDefault.vue'
import ViteEnv from '@/helpers/ViteEnv.ts'; 
console.log(ViteEnv.VITE_APP_BASE_API);
console.log(ViteEnv.VITE_DEBUG);
let pageTitle = ref('Home');
</script>

<template>
    <!-- Breadcrumb Start -->
    <BreadcrumbDefault :pageTitle="pageTitle" />
    <!-- Breadcrumb End -->
    <div
    class="rounded-sm border border-stroke bg-white px-5 pt-6 pb-2.5 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1"
  >
  Github : <a href="https://github.com/mini-software/MiniAuth">https://github.com/mini-software/MiniAuth</a>
    </div>
</template>
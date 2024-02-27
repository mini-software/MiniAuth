<script setup lang="ts">
import { ref } from 'vue'
import { RouterView } from 'vue-router'
import DefaultLayout from './layouts/DefaultLayout.vue'
</script>

<template>
  <DefaultLayout>
      
      <RouterView />
  </DefaultLayout>
</template>

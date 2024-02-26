<script setup lang="ts">
import { ref } from 'vue';
let text = ref(`${'hi'} world!`);
</script>

<template>

    <div class="grid grid-cols-1 gap-4 md:grid-cols-2 md:gap-6 xl:grid-cols-4 2xl:gap-7.5">
        {{ text }}
    </div>
</template>

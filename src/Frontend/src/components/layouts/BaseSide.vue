<template>
  <el-menu
    default-active="2"
    class="el-menu-vertical-demo"
    :collapse="isCollapse"
    @open="handleOpen"
    @close="handleClose"
  >
    <el-menu-item index="1">
      <el-icon><User /></el-icon>
      <template #title>Users</template>
    </el-menu-item>
    <el-menu-item index="2">
      <el-icon><Link /></el-icon>
      <template #title>Routes & Roles</template>
    </el-menu-item>
    <el-menu-item index="3">
      <el-icon><Setting /></el-icon>
      <template #title>Setting</template>
    </el-menu-item>
  </el-menu>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { User,Link,Menu  as MenuIcon, Setting} from "@element-plus/icons-vue";

const isCollapse = ref(true);
const handleOpen = (key: string, keyPath: string[]) => {
  console.log(key, keyPath);
};
const handleClose = (key: string, keyPath: string[]) => {
  console.log(key, keyPath);
};
</script>

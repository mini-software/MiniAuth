<template>
  <div>
    <el-table v-loading="isLoading" :data="tableData" style="width: 100%">
      <el-table-column prop="date" label="Date" width="180" />
      <el-table-column prop="name" label="Name" width="180" />
      <el-table-column prop="address" label="Address" />
    </el-table>
    <el-button @click="handleLoading">Click me</el-button>
    <el-button @click="handleClick">Click me</el-button>
  </div>
</template>


<script setup lang="ts">
import { ref } from "vue";
import { ElMessage } from "element-plus";
const tableData = [
  {
    date: "2016-05-02",
    name: "John Smith",
    address: "No.1518,  Jinshajiang Road, Putuo District",
  },
  {
    date: "2016-05-04",
    name: "John Smith",
    address: "No.1518,  Jinshajiang Road, Putuo District",
  },
  {
    date: "2016-05-01",
    name: "John Smith",
    address: "No.1518,  Jinshajiang Road, Putuo District",
  },
];
const count = ref(0);
const isLoading = ref(false);
const handleClick = () => {
  count.value++;
  ElMessage.success("Hello Element Plus");
};

const handleLoading = () => {
  isLoading.value = true;
  setTimeout(() => {
    isLoading.value = false;
  }, 3000);
};
</script>

<style>
body {
  margin: 0;
}
.example-showcase .el-loading-mask {
  z-index: 9999;
}
</style>


<template>

  <BreadcrumbDefault :pageTitle="pageTitle" />
  <div
    class="rounded-sm border border-stroke bg-white px-5 pt-6 pb-2.5 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
    <table class="w-full table-auto">
      <thead>
        <tr class="bg-gray-2 text-left dark:bg-meta-4">
          <th class="min-w-[220px] py-4 px-4 font-medium text-black dark:text-white xl:pl-11">
            Endpoints
          </th>
          <th class="min-w-[150px] py-4 px-4 font-medium text-black dark:text-white">Method</th>
          <th class="min-w-[120px] py-4 px-4 font-medium text-black dark:text-white">Status</th>
          <th class="py-4 px-4 font-medium text-black dark:text-white">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr class="border-b border-gray-2 dark:border-meta-4" v-for="(item, index) in endpoints" :key="index">
          <td class="py-4 px-4">
            <input type="text" class="form-input mt-2 block w-full" v-model="item.route"
              :disabled="item.type === 'system'"
              :placeholder="(item.route?.length === null || item.route?.length === 0 ? 'Enter Route...' : '')" />
          </td>
          <td class="py-4 px-4">
            <button v-for="(option, index) in ['GET', 'POST', 'PUT', 'PATCH', 'DELETE']" :key="index"
              class="inline-flex rounded-full bg-opacity-10 py-1 px-3 text-sm font-medium" :class="{
    'bg-success text-success':
      item.methods?.find((e) => e === option) ||
      item.methods?.length === 0 ||
      item.methods === null,
    'bg-black ': 1 == 1
  }" @click="setMethod(item.methods, option, index)">
              {{ option }}
            </button>
          </td>
          <td class="py-4 px-4">
            <button class="inline-flex rounded-full bg-opacity-10 py-1 px-3 text-sm font-medium" :class="{
    'bg-success text-success': item.status === 'On',
    'bg-black ': 1 == 1
  }" @click="setStatus(item, 'On')">
              On
            </button>
            <button class="inline-flex rounded-full bg-opacity-10 py-1 px-3 text-sm font-medium" :class="{
    'bg-success text-success': item.status === 'Off',
    'bg-black ': 1 == 1
  }" @click="setStatus(item, 'Off')">
              Off
            </button>
          </td>

          <td class="py-4 px-4">
            <button @click="editItem(index)">Save</button>
            /
            <button @click="deleteItem(index)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>
    <button @click="addItem" class="bg-primary text-white px-4 py-2 rounded-md mt-4">
      Add Item
    </button>
  </div>
</template>


<script setup>
import { reactive, onMounted, ref } from 'vue'
import axios from 'axios'
import BreadcrumbDefault from '@/components/Breadcrumbs/BreadcrumbDefault.vue'
import service from '@/axios/service.ts';

const pageTitle = ref('EndPoints')
const endpoints = ref([])
const fetchData = async () => {
  endpoints.value = await service.get('api/getAllEnPoints')
}

onMounted(async () => {
  await fetchData()
})

const addItem = () => {
  endpoints.value.push({ methods: null, route: '', status: 'On' })
}

const setMethod = (methods, method, index) => {
  if (methods?.find((e) => e === method)) {
    methods.splice(index, 1)
  } else {
    methods.push(method)
  }
}

const setStatus = (item, status) => {
  item.status = status
}

const deleteItem = (index) => {
  endpoints.value.splice(index, 1)
}
</script>

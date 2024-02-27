
<template>
  <BreadcrumbDefault :pageTitle="pageTitle" />
  <div
    class="rounded-sm border border-stroke bg-white px-5 pt-6 pb-2.5 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1"
  >
    <table class="w-full table-auto">
      <thead>
        <tr class="bg-gray-2 text-left dark:bg-meta-4">
          <th class="min-w-[220px] py-4 px-4 font-medium text-black dark:text-white xl:pl-11">
            Route
          </th>
          <th class="min-w-[150px] py-4 px-4 font-medium text-black dark:text-white">Method</th>
          <th class="min-w-[120px] py-4 px-4 font-medium text-black dark:text-white">Status</th>
          <th class="py-4 px-4 font-medium text-black dark:text-white">Actions</th>
        </tr>
      </thead>
      <tbody>
        <!-- tailwind tr class for below simple beauty border support light and dark mode -->
        <tr
          class="border-b border-gray-2 dark:border-meta-4"
          v-for="(item, index) in endpoints"
          :key="index"
        >
          <td class="py-4 px-4">
            <!-- input text for item.route -->
            <!-- /fix input text for item.route  -->
            <!-- please add tailwind input text class -->

            <input type="text" class="form-input mt-2 block w-full" v-model="item.route" 
            :disabled="item.type==='system'"
            :placeholder="(item.route?.length===null||item.route?.length===0?'Enter Route...':'')"/>
        </td>
          <td class="py-4 px-4">
            <button
              v-for="(option, index) in ['GET', 'POST', 'PUT', 'PATCH', 'DELETE']"
              :key="index"
              class="inline-flex rounded-full bg-opacity-10 py-1 px-3 text-sm font-medium"
              :class="{
                'bg-success text-success':
                  item.methods?.find((e) => e === option) ||
                  item.methods?.length === 0 ||
                  item.methods === null,
                'bg-black text-white': 1 == 1
              }"
              @click="setMethod(item.methods, option, index)"
            >
              {{ option }}
            </button>
          </td>
          <td class="py-4 px-4">
            <button
              class="inline-flex rounded-full bg-opacity-10 py-1 px-3 text-sm font-medium"
              :class="{
                'bg-success text-success': item.status === 'On',
                'bg-black text-white': 1 == 1
              }"
              @click="setStatus(item, 'On')"
            >
              On
            </button>
            <button
              class="inline-flex rounded-full bg-opacity-10 py-1 px-3 text-sm font-medium"
              :class="{
                'bg-success text-success': item.status === 'Off',
                'bg-black text-white': 1 == 1
              }"
              @click="setStatus(item, 'Off')"
            >
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


<script setup >
import { reactive, onMounted, ref } from 'vue'
import axios from 'axios'
import BreadcrumbDefault from '@/components/Breadcrumbs/BreadcrumbDefault.vue'

const pageTitle = ref('Routes')

const apiClient = axios.create({
  baseURL: 'http://localhost:5566/MiniAuth/', // 設置你的 API 基礎 URL
  timeout: 5000, // 設置請求超時時間（毫秒）
  headers: {
    'Content-Type': 'application/json',
    // 如果你的 API 需要認證，你可以在這裡設置默認的認證信息
    'X-MiniAuth-Token':
      'eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJtaW5pYXV0aCIsIm5hbWUiOiJtaW5pYXV0aCIsImlzcyI6Im1pbmlhdXRoIiwicm9sZXMiOlsiYWRtaW4iXSwianRpIjoiYmE0MjZlYzgtMzBlMy00NmQzLTlmNzUtYzdmNjQxYTNmYTQyIiwiZXhwIjoxNzA5MDkyMjU4LCJpYXQiOjE3MDkwMDU4NTh9.XX4zYO9yYJyCfqf2pilH2trrEK9JBJrbK0j0HYsLC2--0kMpU_mY02vVQ7dWtkwSfY6zAFQNGxvVpcAU0ig_2qIHgfGd6__MJeyKoQuTEVjhlPy2no7G09Fqp4L0YnMT-39K9J8WEf-30TLqdX4A4YC8xqzJzK3VsTkBcBtmqf-iCs8I0B80_DubqCb942OL5Cy5QB-2yO489Y9Y4BKdnKMc0o920b9Dbm-j_onvvnZoKBrgXBzLUT0WY0VNzNiWDK_svUdS0-qaHiEZibdbQ0tDfNrBQ6XWiIe9yrpcCnr5gpIpM4PvVR-In3Sw47pQBCTvawrE2K8SzCvAv0Higg'
  }
})

const endpoints = ref([])
const fetchData = async () => {
  try {
    const response = await apiClient.get('api/getAllEnPoints')
    endpoints.value = response.data
  } catch (error) {
    console.error('Error fetching data:', error)
  }
}

onMounted(async () => {
  await fetchData()
})

const addItem = () => {
  endpoints.value.push({ methods: null, route: '',status:'On' })
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

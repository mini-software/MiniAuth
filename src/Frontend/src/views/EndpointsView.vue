<template>
  <div>
    <table class="table">
      <thead>
        <tr>
          <th>
            Endpoints
          </th>
          <th>Type</th>
          <th>Method</th>
          <th>Enable</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in endpoints" :key="index">
          <td>
            <input type="text"
               v-model="item.Route"
              :disabled="item.Type === 'system' || item.Type === 'miniauth'"
              :placeholder="(item.route?.length === null || item.route?.length === 0 ? 'Enter Route...' : '')" />
          </td>
          <td>
            {{ item.Type }}
          </td>
          <td>
            <select multiple v-model="item.Methods" :disabled="item.Type === 'system' || item.Type === 'miniauth'" style="height: 50px;">
              <option v-for="(option, index) in ['GET', 'POST', 'PUT', 'PATCH', 'DELETE']" :key="index">{{option}}</option>
            </select>
          </td>
          <td >
            <input type="checkbox" v-model="item.Enable">
          </td>

          <td >
            <button @click="editItem(index)">Save</button>
            /
            <button @click="deleteItem(index)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>
    <button @click="addItem" >
      Add Item
    </button>
  </div>
</template>


<script setup>
import { onMounted, ref } from 'vue'
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

const setStatus = (item, status) => {
  item.status = status
}

const deleteItem = (index) => {
  endpoints.value.splice(index, 1)
}
</script>

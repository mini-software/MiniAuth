<template>
  <div>
    <table class="table">
      <thead>
        <tr>
          <th>
            Name
          </th>
          <th>
            Route
          </th>
          <th>Redirect</th>
          <th>Enable</th>
          <th>Roles</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <tr  v-for="(item, index) in endpoints" :key="index">
          <td>
            {{ item.Name}} 
          </td>
          <td>
            {{ item.Route }}
          </td>
          <td >
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.RedirectToLoginPage">
            </div>
          </td>

          <td >
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.Enable">
            </div>
          </td>
          <td>
            
          </td>
          <td >
            <button class="btn btn-success" @click="saveEndpoint(item)">Save</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style> 
     .resizable {
         height: 30px !important;
         transition: height 0.3s ease; 
         overflow: hidden; 
     }

         .resizable:hover {
             height: 130px !important;
         }
</style>

<script setup>
import { onMounted, ref } from 'vue'
import service from '@/axios/service.ts';
const pageTitle = ref('EndPoints')
const endpoints = ref([])
const fetchData = async () => {
  endpoints.value = await service.get('api/getAllEndpoints')
}
const saveEndpoint = async (endpoint) => {
  await service.post('api/saveEndpoint', endpoint).then(() => {
    alert("updated successfully")
  })
}
onMounted(async () => {
  await fetchData()
})
</script>

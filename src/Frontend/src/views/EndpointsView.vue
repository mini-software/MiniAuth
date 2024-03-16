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
            <select multiple v-model="item.Roles"  class="resizable" style="height: 25px;">
              <option key="0"></option>
              <option v-for="(role, index) in roles" :value="role.Id" :key="index">{{role.Name}}</option>
            </select>
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
const roles = ref([])
const fetchData = async () => {
  endpoints.value = await service.get('api/getAllEndpoints')
  roles.value = await service.get('api/getRoles')
}
const saveEndpoint = async (endpoint) => {
  if(!confirm("Are you sure you want to update this endpoint?")){
    return;
  }
  await service.post('api/saveEndpoint', endpoint).then(() => {
    alert("updated successfully")
  })
}
onMounted(async () => {
  await fetchData()
})
</script>

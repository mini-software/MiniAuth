<template>
  <div>
    <div class="row" style="padding-bottom: 10px;padding-top: 10px;">
      <div class="col-sm-8">
        <button @click="insert" class="  " type="button">
          <svg width="40px" height="40px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M7 12L12 12M12 12L17 12M12 12V7M12 12L12 17" stroke="#000000" stroke-width="2"
              stroke-linecap="round" stroke-linejoin="round" />
            <circle cx="12" cy="12" r="9" stroke="#000000" stroke-width="2" stroke-linecap="round"
              stroke-linejoin="round" />
          </svg>
          <span class="visually-hidden">Insert</span>
        </button>
        <button @click="fetchData" class="btn  " type="button">
          <svg fill="#000000" width="40px" height="40px" viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg"><path d="M27.1 14.313V5.396L24.158 8.34c-2.33-2.325-5.033-3.503-8.11-3.503C9.902 4.837 4.901 9.847 4.899 16c.001 6.152 5.003 11.158 11.15 11.16 4.276 0 9.369-2.227 10.836-8.478l.028-.122h-3.23l-.022.068c-1.078 3.242-4.138 5.421-7.613 5.421a8 8 0 0 1-5.691-2.359A7.993 7.993 0 0 1 8 16.001c0-4.438 3.611-8.049 8.05-8.049 2.069 0 3.638.58 5.924 2.573l-3.792 3.789H27.1z"/></svg>
          <span class="visually-hidden">Insert</span>
        </button>
      </div>
      <div class="col-sm-4">

      </div>
    </div>
    <table class="table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Name</th>
          <th>Enable</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in roles" :key="index">
          <td>
            {{ item.Id }}
          </td>
          <td>
            <input type="text" v-model="item.Name">
          </td>
          <td>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.Enable">
            </div>
          </td>
          <td>
            <button class="btn btn-success" @click="save(item)">Save</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style>
.btn:hover{
  background-color: rgb(57, 56, 56);
}

</style>

<script setup>
import { onMounted, ref } from 'vue'
import service from '@/axios/service.ts';
const pageTitle = ref('Roles')
const roles = ref([])
const fetchData = async () => {
  roles.value = await service.get('api/getRoles')
}
const insert = async () => {
  if (!confirm("Are you sure you want to insert?")) {
    return;
  }
  roles.value.push({ Id: null, Name: '', Enable: true })
}
const save = async (data) => {
  if (!confirm("Are you sure you want to update?")) {
    return;
  }
  await service.post('api/saveRole', data).then(async () => {
    alert("updated successfully")
    await fetchData();
  })
}
onMounted(async () => {
  await fetchData()
})
</script>

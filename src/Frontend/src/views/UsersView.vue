<template>
  <div>
    <div class="row" style="padding-bottom: 10px;padding-top: 10px;">
      <div class="col-sm-8">
        <button @click="insert" class="btn" type="button">
          <svg width="40px" height="40px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M7 12L12 12M12 12L17 12M12 12V7M12 12L12 17" stroke="#000000" stroke-width="2"
              stroke-linecap="round" stroke-linejoin="round" />
            <circle cx="12" cy="12" r="9" stroke="#000000" stroke-width="2" stroke-linecap="round"
              stroke-linejoin="round" />
          </svg>
          <span class="visually-hidden">Insert</span>
        </button>
        <button @click="fetchData" class="btn  " type="button">
          <svg fill="#000000" width="40px" height="40px" viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M27.1 14.313V5.396L24.158 8.34c-2.33-2.325-5.033-3.503-8.11-3.503C9.902 4.837 4.901 9.847 4.899 16c.001 6.152 5.003 11.158 11.15 11.16 4.276 0 9.369-2.227 10.836-8.478l.028-.122h-3.23l-.022.068c-1.078 3.242-4.138 5.421-7.613 5.421a8 8 0 0 1-5.691-2.359A7.993 7.993 0 0 1 8 16.001c0-4.438 3.611-8.049 8.05-8.049 2.069 0 3.638.58 5.924 2.573l-3.792 3.789H27.1z" />
          </svg>
          <span class="visually-hidden">Insert</span>
        </button>
      </div>
      <div class="col-sm-4">

      </div>
    </div>
    <table class="table">
      <thead>
        <tr class="table-dark">
          <th>ID</th>
          <th>User Name</th>
          <th>Roles</th>
          <th>First Name</th>
          <th>Last Name</th>
          <th hidden>Email</th>
          <th>New Password</th>
          <th>Enable</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in users" :key="index">
          <td>
            {{ item.Id }}
          </td>
          <td>
            <input type="text" v-model="item.Username">
          </td>
          <td>
            <select multiple v-model="item.Roles" class="resizable" style="height: 25px;">
              <option></option>
              <option v-for="(role, index) in roles" :value="role.Id" :key="index">{{ role.Name }}</option>              
            </select>
          </td>
          <td>
            <input type="text" v-model="item.First_name"> 
          </td>
          <td>
            <input type="text" v-model="item.Last_name">
          </td>
          <td hidden>
            <input type="mail" v-model="item.Mail">
          </td>
          <td>
            <input type="password" v-model="item.NewPassword">
          </td>
          <td>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.Enable">
            </div>
          </td>
          <td>
            <button class="btn" @click="deleteRole(item.Id)">
              <svg width="20px" height="20px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path
                  d="M13.0151 13.6556C14.8093 14.3587 16.9279 13.9853 18.3777 12.5355C20.3304 10.5829 20.3304 7.41709 18.3777 5.46447C16.4251 3.51184 13.2593 3.51184 11.3067 5.46447C9.85687 6.91426 9.48353 9.03288 10.1866 10.8271M12.9964 13.6742L6.82843 19.8422L4.2357 19.6065L4 17.0138L10.168 10.8458M15.5493 8.31568V8.29289"
                  stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
              </svg>
            </button>
            <button class="btn" @click="save(item)">
              <svg width="20px" height="20px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <g id="System / Save">
                  <path id="Vector"
                    d="M17 21.0002L7 21M17 21.0002L17.8031 21C18.921 21 19.48 21 19.9074 20.7822C20.2837 20.5905 20.5905 20.2843 20.7822 19.908C21 19.4806 21 18.921 21 17.8031V9.21955C21 8.77072 21 8.54521 20.9521 8.33105C20.9095 8.14 20.8393 7.95652 20.7432 7.78595C20.6366 7.59674 20.487 7.43055 20.1929 7.10378L17.4377 4.04241C17.0969 3.66374 16.9242 3.47181 16.7168 3.33398C16.5303 3.21 16.3242 3.11858 16.1073 3.06287C15.8625 3 15.5998 3 15.075 3H6.2002C5.08009 3 4.51962 3 4.0918 3.21799C3.71547 3.40973 3.40973 3.71547 3.21799 4.0918C3 4.51962 3 5.08009 3 6.2002V17.8002C3 18.9203 3 19.4796 3.21799 19.9074C3.40973 20.2837 3.71547 20.5905 4.0918 20.7822C4.5192 21 5.07899 21 6.19691 21H7M17 21.0002V17.1969C17 16.079 17 15.5192 16.7822 15.0918C16.5905 14.7155 16.2837 14.4097 15.9074 14.218C15.4796 14 14.9203 14 13.8002 14H10.2002C9.08009 14 8.51962 14 8.0918 14.218C7.71547 14.4097 7.40973 14.7155 7.21799 15.0918C7 15.5196 7 16.0801 7 17.2002V21M15 7H9"
                    stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                </g>
              </svg>
            </button>
            <button class="btn" @click="deleteRole(item.Id)"><svg width="20px" height="20px" viewBox="0 0 24 24"
                fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M10 12V17" stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                <path d="M14 12V17" stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                <path d="M4 7H20" stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                <path d="M6 10V18C6 19.6569 7.34315 21 9 21H15C16.6569 21 18 19.6569 18 18V10" stroke="#000000"
                  stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                <path d="M9 5C9 3.89543 9.89543 3 11 3H13C14.1046 3 15 3.89543 15 5V7H9V5Z" stroke="#000000"
                  stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
              </svg></button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
input[type="text"] {
  widows: 100%;
  border: 0;
  border-bottom: 1px solid black;
  outline: 0;
  background-color: rgba(226, 226, 226, 0.744);
}
input[type="password"] {
  widows: 100%;
  border: 0;
  border-bottom: 1px solid black;
  outline: 0;
  background-color: rgba(226, 226, 226, 0.744);
}

input[type="mail"] {
  widows: 100%;
  border: 0;
  border-bottom: 1px solid black;
  outline: 0;
  background-color: rgba(226, 226, 226, 0.744);
}

.resizable {
  height: 26px !important;
  transition: height 0.3s ease;
  border: 0; 
  border-bottom: 1px solid black; 
}

.resizable:hover {
  height: 100% !important;
}
</style>

<script setup>
import { onMounted, ref } from 'vue'
import service from '@/axios/service.ts';
const pageTitle = ref('Users')
const users = ref([])
const roles = ref([])
const fetchData = async () => {
  users.value = await service.get('api/getUsers')
  roles.value = await service.get('api/getRoles')
}
const insert = async () => {
  if (!confirm("Are you sure you want to insert?")) {
    return;
  }
  users.value.push({ Id: null, Enable: true })
}
const deleteRole = async (Id) => {
  if (!confirm("Are you sure you want to delete?")) {
    return;
  }
  await service.post('api/deleteRole', { Id: Id }).then(async () => {
    alert("Delete successfully")
    await fetchData();
  })
}
const save = async (data) => {
  if (!confirm("Are you sure you want to update?")) {
    return;
  }
  await service.post('api/saveUser', data).then(async () => {
    alert("updated successfully")
    await fetchData();
  })
}
onMounted(async () => {
  await fetchData()
})
</script>

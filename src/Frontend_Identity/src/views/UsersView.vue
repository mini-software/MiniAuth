<template>
  <div class="scrollable-container">
    <div class="row" style="padding-bottom: 10px;padding-top: 10px;">
      <div class="col-sm-8">
        <button :title="$t('add')" @click="insert" class="btn" type="button">
          <svg width="40px" height="40px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M7 12L12 12M12 12L17 12M12 12V7M12 12L12 17" stroke="#000000" stroke-width="2"
              stroke-linecap="round" stroke-linejoin="round" />
            <circle cx="12" cy="12" r="9" stroke="#000000" stroke-width="2" stroke-linecap="round"
              stroke-linejoin="round" />
          </svg>
          <span class="visually-hidden">Insert</span>
        </button>
        <button :title="$t('refresh')" @click="fetchData" class="btn  " type="button">
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
    <table class="table table-hover">
      <thead>
        <tr class="table-dark">
          <th>{{ $t("Action") }}</th>
          <th>{{ $t("UserName") }}</th>
          <th>{{ $t("Roles") }}</th>
          <th>{{ $t("Email") }}</th> 
          <th>{{ $t("PhoneNumber") }}</th> 
          <th>{{ $t("FirstName") }}</th>
          <th>{{ $t("LastName") }}</th>
          <th>{{ $t("Employee_Number") }}</th>
          <th>{{ $t("Enable") }}</th>
          <th>{{ $t("Email Confirmed") }}</th> 
          <th>{{ $t("PhoneNumber Confirmed") }}</th> 
          <th>{{ $t("Two Factor Enabled") }}</th> 
          <th>{{ $t("Lockout Enabled") }}</th> 
          <th>{{ $t("Lockout End") }}</th> 
          <th>{{ $t("Access Failed Count") }}</th> 
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in users" :key="index">
          <td>
            <button class="btn" @click="openEditModal(item)">
              <img src="../assets/svg/edit.svg" />
            </button>
            <button class="btn" @click="resetPassword(item)">
              <img src="../assets/svg/reset_password.svg" />
            </button>
            <button :title="$t('Save')" class="btn" @click="save(item)">
              <img src="../assets/svg/save.svg" />
            </button>
            <button :disabled="item?.Type == 'miniauth'" class="btn" @click="deleteUser(item.Id)">
              <img src="../assets/svg/delete.svg" />
            </button>
          </td>          
          <td>
            <input class="input_no_border" type="text" v-model="item.Username">
          </td>
          <td>
            <div class="resizable" >
              <div class=" form-check" v-for="(role, index) in roles" :key="index">
                <input :disabled="item.Type == 'miniauth' || role.Enable == false" class="role_checkbox form-check-input"
                  type="checkbox" :value="role.Id" v-model="item.Roles">
                <label class="form-check-label" :for="'role_' + index">{{ role.Name }}</label>
              </div>
            </div>
          </td>
          <td>
            <input class="input_no_border" type="mail" v-model="item.Mail">
          </td>
          <td>
            <input class="input_no_border" type="text" v-model="item.PhoneNumber">
          </td>
          <td>
            <input class="input_no_border" type="text" v-model="item.First_name">
          </td>
          <td>
            <input class="input_no_border" type="text" v-model="item.Last_name">
          </td> 
          <td>
            <input class="input_no_border" type="text" v-model="item.Emp_no">
          </td>
          <td>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.Enable">
            </div>
          </td>
          <td>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.EmailConfirmed">
            </div>
          </td>
          <td>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.PhoneNumberConfirmed">
            </div>
          </td>
          <td>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.TwoFactorEnabled">
            </div>
          </td>
          <td>
            <div class="form-check form-switch">
              <input class="form-check-input" type="checkbox" v-model="item.LockoutEnabled">
            </div>
          </td>
          <td>
            <input class="input_no_border" type="datetime-local" v-model="item.LockoutEnd">
          </td>
          <td>
            <input readonly class="input_no_border" type="text" v-model="item.AccessFailedCount">
          </td>
        </tr>
      </tbody>
    </table>

    <nav aria-label="Page navigation">
      <ul class="pagination justify-content-center">
        <li class="page-item" :class="{ 'disabled': pageIndex === 0 }">
          <button class="page-link" @click.prevent="goToPage(pageIndex - 1)">{{ $t("Previous") }}</button>
        </li>
        <li class="page-item" :class="{ 'active': pageIndex === currentIndex }"
          v-for="(page, currentIndex) in computedPages" :key="currentIndex">
          <button class="page-link" @click.prevent="goToPage(currentIndex)">{{ currentIndex + 1 }}</button>
        </li>
        <li class="page-item" :class="{ 'disabled': pageIndex >= (Math.ceil(totalItems / pageSize) - 1) }">
          <button class="page-link" @click.prevent="goToPage(pageIndex + 1)">{{ $t("Next") }}</button>
        </li>
      </ul>
    </nav>

    <div>
      <div class="modal fade" id="editmodal" tabindex="-1" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog  modal-dialog-centered">
          <div class="modal-content ">
            <div class="modal-header bg-white">
              <h5 class="modal-title ">
                <svg width="40px" height="40px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path
                    d="M11 15C10.1183 15 9.28093 14.8098 8.52682 14.4682C8.00429 14.2315 7.74302 14.1131 7.59797 14.0722C7.4472 14.0297 7.35983 14.0143 7.20361 14.0026C7.05331 13.9914 6.94079 14 6.71575 14.0172C6.6237 14.0242 6.5425 14.0341 6.46558 14.048C5.23442 14.2709 4.27087 15.2344 4.04798 16.4656C4 16.7306 4 17.0485 4 17.6841V19.4C4 19.9601 4 20.2401 4.10899 20.454C4.20487 20.6422 4.35785 20.7951 4.54601 20.891C4.75992 21 5.03995 21 5.6 21H8.4M15 7C15 9.20914 13.2091 11 11 11C8.79086 11 7 9.20914 7 7C7 4.79086 8.79086 3 11 3C13.2091 3 15 4.79086 15 7ZM12.5898 21L14.6148 20.595C14.7914 20.5597 14.8797 20.542 14.962 20.5097C15.0351 20.4811 15.1045 20.4439 15.1689 20.399C15.2414 20.3484 15.3051 20.2848 15.4324 20.1574L19.5898 16C20.1421 15.4477 20.1421 14.5523 19.5898 14C19.0376 13.4477 18.1421 13.4477 17.5898 14L13.4324 18.1574C13.3051 18.2848 13.2414 18.3484 13.1908 18.421C13.1459 18.4853 13.1088 18.5548 13.0801 18.6279C13.0478 18.7102 13.0302 18.7985 12.9948 18.975L12.5898 21Z"
                    stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
              </h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
              <form v-if="isModalOpen">
                <label for="userName">{{ $t("UserName") }}:</label>
                <input class="form-control" type="text" v-model="editedUser.Username" id="userName" required>

                <label for="roles">{{ $t("Roles") }}:</label>
                <div style="height: 100px;scroll-behavior: smooth;overflow-y: auto;">
                  <div class=" form-check" v-for="(role, index) in roles" :key="index">
                    <input :disabled="editedUser.Type == 'miniauth' || role.Enable == false"
                      class="role_checkbox form-check-input" type="checkbox" :value="role.Id"
                      v-model="editedUser.Roles">
                    <label class="form-check-label" :for="'role_' + index">{{ role.Name }}</label>
                  </div>
                </div>

                <label for="firstName">{{ $t("FirstName") }}:</label>
                <input class="form-control" type="text" v-model="editedUser.First_name" id="firstName" required>
                <label for="lastName">{{ $t("LastName") }}:</label>
                <input class="form-control" type="text" v-model="editedUser.Last_name" id="lastName" required>
                <label for="email">Email:</label>
                <input class="form-control" type="email" v-model="editedUser.Mail" id="email" required>
                <label for="empNo">{{ $t("Employee_Number") }}:</label>
                <input class="form-control" type="text" v-model="editedUser.Emp_no" id="empNo" required>

                <label for="enable">{{ $t("Enable") }}:</label>
                <div class="form-check form-switch">
                  <input class="form-check-input" type="checkbox"
                    v-model="editedUser.Enable">
                </div>
              </form>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                {{ $t("Cancel") }}
              </button>
              <button type="button" @click="save(editedUser)" class="btn btn-primary">{{ $t("Save") }}</button>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
.page-item.active .page-link {
  color: #fff !important;
  background: black !important;
  --bs-pagination-active-border-color: black;
}

.page-link {
  color: black !important;
}



.password {
  widows: 100%;
  border: 0;
  border-bottom: 1px solid black;
  outline: 0;
  background-color: rgba(226, 226, 226, 0.744);
}

input[type="mail"] {
  widows: 100%;
  border: 0;
  /* border-bottom: 1px solid black; */
  outline: 0;
  /* background-color: rgba(226, 226, 226, 0.744); */
}
/* input type=text or mail */

</style>

<script setup>
import { computed, onMounted, ref } from 'vue'
import service from '@/axios/service.ts';
import { i18n } from '@/i18n'
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const pageTitle = ref('Users')
const users = ref([])
const roles = ref([])
const pageSize = ref(10)
const pageIndex = ref(0)
const totalItems = ref(0)

const goToPage = (index) => {
  pageIndex.value = index
  fetchData()
}
const computedPages = computed(() => {
  const totalPages = Math.ceil(totalItems.value / pageSize.value);
  return Array.from({ length: totalPages }, (_, index) => index);
})
const fetchData = async () => {
  await service.post('api/getUsers', { pageSize: pageSize.value, pageIndex: pageIndex.value }).then(res => {
    totalItems.value = res.totalItems
    users.value = res.users
    return res.users
  })
  roles.value = await service.get('api/getRoles')
}

const isModalOpen = ref(false)
const editedUser = ref(null)
var editModal = null;
const openEditModal = (user) => {
  editedUser.value = { ...user }
  isModalOpen.value = true
  if (editModal == null) {
    editModal = new bootstrap.Modal(document.getElementById('editmodal'), {
      keyboard: false
    })
  }
  editModal.show()
}

const closeEditModal = () => {
  editedUser.value = null
  isModalOpen.value = false
  editModal.hide();
}

const insert = async () => {
  if (!confirm("Are you sure you want to insert?")) {
    return;
  }
  users.value.push({ Id: null, Enable: true, Roles: [] })
}

const deleteUser = async (Id) => {
  if (!confirm("Are you sure you want to delete?")) {
    return;
  }
  await service.post('api/deleteUser', { Id: Id }).then(async () => {
    alert("Delete successfully")
    await fetchData();
  })
}
const save = async (data) => {
  if (!confirm(t("please_confirm"))) {
    return;
  }
  await service.post('api/saveUser', data).then(async (res) => {
    alert(t("updated_successfully"))

    if (data.Id == null || data.Id == undefined) {
      alert(t("new_password", [res.newPassword]))
      navigator.clipboard.writeText(res.newPassword)
    }
    await fetchData();
  })
}
const resetPassword = async (data) => {
  if (!confirm(t("resetPasswordConfirm"))) {
    return;
  }
  await service.post('api/resetPassword', data).then(async (res) => {
    alert(t("new_password", [res.newPassword]))
    navigator.clipboard.writeText(res.newPassword)
  })
}
onMounted(async () => {
  await fetchData()
})
</script>

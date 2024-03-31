<script setup lang="ts">
import { watch, onMounted, ref } from 'vue'
import { RouterView } from 'vue-router'
import { emitter } from '@/helpers/emitter'
import { useRoute } from 'vue-router';
const route = useRoute();
const routeName = ref("");

onMounted(() => {
  routeName.value = (route.name ?? '').toString();
});

watch(() => route.name, (newVal) => {
  routeName.value = newVal?.toString() ?? "";
});


const logout = () => {
  localStorage.removeItem('X-MiniAuth-Token')
  document.cookie = 'X-MiniAuth-Token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
  window.location.href = '/miniauth/login.html'
}

const loadingFlag = ref(false)
emitter.on('showLoading', () => {
  loadingFlag.value = true
})
emitter.on('closeLoading', () => {
  loadingFlag.value = false
})
</script>

<template>
  <div>
    <div>
      <SidebarArea />
      <div>
        <div>
          <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
            <div class="container scrollable-container">
              <router-link class="navbar-brand" to="/"> MiniAuth </router-link>
              <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav"
                aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
              </button>
              <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav">
                  <li class="nav-item">
                    <router-link class="nav-link" to="/Endpoints"> Endpoints </router-link>
                  </li>
                  <li class="nav-item">
                    <router-link class="nav-link " to="/Users"> Users </router-link>
                  </li>
                  <li class="nav-item">
                    <router-link class="nav-link " to="/Roles"> Roles </router-link>
                  </li>
                </ul>
                <div class="navbar-nav ms-auto">
                  <div @click="logout" class="btn nav-item nav-link">Logout</div>
                </div>
              </div>
            </div>
          </nav>
        </div>
        <main class="container scrollable-container">
          <div class="row" style="padding-bottom: 10px;padding-top: 10px;">
            <div class="col-sm-8">
              <h2><b>{{ routeName }}</b> Management</h2>
            </div>
            <div class="col-sm-4">
            </div>
          </div>
          <div>
            <RouterView />
          </div>
        </main>
      </div>
    </div>
  </div>
  <div v-show="loadingFlag">
    <div id="loading-mask">
      <div class="preloader">
        <div class="c-three-dots-loader"></div>
      </div>
    </div>
  </div>
</template>

<style>
/* scrollable-container */
.scrollable-container {
  overflow-x: auto;
  white-space: nowrap;
}

.scrollable-container::-webkit-scrollbar {
  width: 10px;
}

.scrollable-container::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.scrollable-container::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 10px;
}

/*  */
.btn:disabled {
  border-color: #ffffff !important;
}

#loading-mask {
  width: 100vw;
  height: 100%;
  overflow: hidden;
  position: absolute;
  background-color: #000;
  bottom: 0;
  left: 0;
  right: 0;
  top: 0;
  z-index: 9999;
  opacity: 0.8;
}

#loading-mask h1 {
  text-align: center;
  color: black;
  padding-top: 50%;
  transform: translateY(-50%);
}

#loading-mask .preloader {
  position: absolute;
  text-align: center;
  height: 20px;
  width: 20px;
  top: calc(50vh - 10px);
  left: calc(50vw - 10px);
}

#loading-mask .c-three-dots-loader {
  position: relative;
  display: inline-block;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  animation-fill-mode: both;
  animation: three-dots-loader-animation 2s infinite ease-in-out;
  animation-delay: -0.16s;
  color: #333;
}

#loading-mask .c-three-dots-loader:before,
#loading-mask .c-three-dots-loader:after {
  content: '';
  position: absolute;
  width: 20px;
  height: 20px;
  top: 0;
  animation: three-dots-loader-animation 2s infinite ease-in-out;
  border-radius: 50%;
}

#loading-mask .c-three-dots-loader:before {
  left: -24px;
  animation-delay: -0.32s;
}

#loading-mask .c-three-dots-loader:after {
  left: 24px;
}

#loading-mask .load-mask-wrapper {
  position: absolute;
  text-align: center;
  height: 200px;
  width: 200px;
  top: calc(50vh - 100px);
  left: calc(50vw - 100px);
}

#loading-mask .load-mask-innerDots {
  position: absolute;
  top: 6px;
  left: 6px;
  width: 100%;
  height: 100%;
}

#loading-mask .load-mask-outerPlane {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
}

@keyframes three-dots-loader-animation {

  0%,
  80%,
  100% {
    box-shadow: 0 20px 0 -24px;
  }

  40% {
    box-shadow: 0 20px 0 0;
  }
}
</style>

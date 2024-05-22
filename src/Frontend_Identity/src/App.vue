<script setup lang="ts">
import { watch, onMounted, ref } from 'vue'
import { RouterView } from 'vue-router'
import { emitter } from '@/helpers/emitter'
import { useRoute } from 'vue-router';
import { i18n } from '@/i18n'
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const route = useRoute();
const routeName = ref("");

onMounted(() => {
  routeName.value = (route.meta.title ?? '').toString();
});

watch(() => route.name, (newVal) => {
  routeName.value = newVal?.toString() ?? "";
});


const logout = () => {
  if (!confirm(t('LogoutMessage'))) {
    return;
  }
  localStorage.removeItem('X-MiniAuth-Token')
  document.cookie = 'X-MiniAuth-Token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
  window.location.href = '/miniauth/login.html'
}

const switchLang = (lang) => {
  i18n.global.locale.value = lang ?? 'en_us';
  localStorage.setItem('lang', i18n.global.locale.value)
}

const loadingFlag = ref(false)
emitter.on('showLoading', () => {
  loadingFlag.value = true
})
emitter.on('closeLoading', () => {
  loadingFlag.value = false
})

const setLanguage = () => {
  const browserLang = navigator.language || navigator.userLanguage;
  const storedLang = localStorage.getItem('lang');
  const defaultLang = 'en_us';
  const browserLangCode = browserLang.toLowerCase().replace('-', '_');
  const langCode = browserLangCode.split('_')[0];
  if (storedLang) {
    switchLang(storedLang);
    return;
  }

  if (browserLangCode === 'zh_cn' || browserLangCode === 'zh_hans') {
    switchLang('zh_cn');
    return;
  }
  if (langCode === 'zh') {
    switchLang('zh_hant');
    return;
  }
  if (langCode === 'en') {
    switchLang('en_us');
    return;
  }
  if (langCode === 'ja') {
    switchLang('ja');
    return;
  }
  if (langCode === 'ko') {
    switchLang('ko');
    return;
  }
  if (langCode === 'es') {
    switchLang('es');
    return;
  }
  if (langCode === 'fr') {
    switchLang('fr');
    return;
  }
  if (langCode === 'ru') {
    switchLang('ru');
    return;
  }

  switchLang(defaultLang);
}

onMounted(() => {
  setLanguage();
});


</script>

<template>
  <div>
    <div>
      <SidebarArea />
      <div>
        <div>
          <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
            <div class="container ">
              <router-link class="navbar-brand" to="/"> MiniAuth </router-link>
              <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav"
                aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
              </button>
              <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav">
                  <li class="nav-item">
                    <router-link class="nav-link" to="/"> {{ $t("Endpoints") }} </router-link>
                  </li>
                  <li class="nav-item">
                    <router-link class="nav-link " to="/Users"> {{ $t("Users") }} </router-link>
                  </li>
                  <li class="nav-item">
                    <router-link class="nav-link " to="/Roles"> {{ $t("Roles") }} </router-link>
                  </li>
                </ul>
                <div class="navbar-nav ms-auto">
                  <ul class="navbar-nav">
                    <li class="nav-item dropdown">
                      <a class="nav-link dropdown-toggle" href="#" id="navbarDropdownMenuLink" role="button"
                        data-bs-toggle="dropdown" aria-expanded="false">
                        {{ $t("Lang") }}
                      </a>
                      <ul class="dropdown-menu" aria-labelledby="navbarDropdownMenuLink">
                        <li><a class="btn dropdown-item" @click="switchLang('en_us')">English</a></li>
                        <li><a class="btn dropdown-item" @click="switchLang('zh_cn')">简体中文</a></li>
                        <li><a class="btn dropdown-item" @click="switchLang('zh_hant')">繁體中文</a></li>
                        <li><a class="btn dropdown-item" @click="switchLang('ja')">日本語</a></li>
                        <li><a class="btn dropdown-item" @click="switchLang('ko')">한국어</a></li>
                        <li><a class="btn dropdown-item" @click="switchLang('es')">Español</a></li>
                        <li><a class="btn dropdown-item" @click="switchLang('fr')">Français</a></li>
                        <li><a class="btn dropdown-item" @click="switchLang('ru')">Русский</a></li>
                      </ul>
                    </li>
                  </ul>

                  <div @click="logout" class="nav-item nav-link" style="cursor: pointer;">
                    {{ $t("Logout") }}
                  </div>
                </div>
              </div>

            </div>
          </nav>
        </div>
        <main class="container scrollable-container">
          <div class="row" style="padding-bottom: 10px;padding-top: 10px;">
            <div class="col-sm-8">
              <h2><b>{{ routeName }}</b> {{ $t("Management") }}</h2>
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

/* ---- */
.resizable {
  height: 25px !important;
  width:100%;
  scroll-behavior: smooth;
  /* overflow-y: auto; */
  transition: height 0.3s ease;
  overflow: auto;
}

.resizable:hover {
  height: 130px !important;
}

/* ---- */

.input_no_border {
  widows: 100%;
  border: 0;
  outline: 0;
}

/* ---- */
.role_checkbox {
  border: 1px solid #000;
  appearance: none;
  position: relative;
}

.role_checkbox:checked::after {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  text-align: center;
}


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

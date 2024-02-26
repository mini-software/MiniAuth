import { createRouter, createWebHistory } from 'vue-router'
import Routes from '@/views/Routes.vue'
import SettingsView from '@/views/Pages/SettingsView.vue'
import ProfileView from '@/views/ProfileView.vue'

const routes = [
  {
    path: '/',
    name: 'Routes',
    component: Routes,
    meta: {
      title: 'Routes'
    }
  },
  {
    path: '/profile',
    name: 'profile',
    component: ProfileView,
    meta: {
      title: 'Profile'
    }
  },
  {
    path: '/pages/settings',
    name: 'settings',
    component: SettingsView,
    meta: {
      title: 'Settings'
    }
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(to, from, savedPosition) {
    return savedPosition ?? { left: 0, top: 0 }
  }
})

router.beforeEach((to, from, next) => {
  document.title = `MiniAuth ${to.meta.title} `
  next()
})

export default router

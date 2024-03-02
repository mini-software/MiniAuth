import { createRouter, createWebHistory } from 'vue-router'
import EndpointsView from '@/views/EndpointsView.vue'
import HomeView from '@/views/HomeView.vue'
import SettingsView from '@/views/SettingsView.vue'
import ProfileView from '@/views/ProfileView.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: HomeView,
    meta: {
      title: 'Home'
    }
  },
  {
    path: '/endpoints',
    name: 'Endpoints',
    component: EndpointsView,
    meta: {
      title: 'Endpoints'
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
    path: '/settings',
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

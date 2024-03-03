import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/HomeView.vue'),
    meta: {
      title: 'Home'
    }
  },
  {
    path: '/endpoints',
    name: 'Endpoints',
    component: () => import('@/views/EndpointsView.vue'),
    meta: {
      title: 'Endpoints'
    }
  },  
  {
    path: '/profile',
    name: 'profile',
    component: () => import('@/views/ProfileView.vue'),
    meta: {
      title: 'Profile'
    }
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('@/views/SettingsView.vue'),
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

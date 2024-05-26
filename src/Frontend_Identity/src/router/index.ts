import { createRouter, createWebHashHistory, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Endpoints',
    component: () => import('@/views/EndpointsView.vue'),
    meta: {
      title: 'Endpoints'
    }
  },
  {
    path: '/roles',
    name: 'Roles',
    component: () => import('@/views/RolesView.vue'),
    meta: {
      title: 'Roles'
    }
  },
  {
    path: '/users',
    name: 'Users',
    component: () => import('@/views/UsersView.vue'),
    meta: {
      title: 'Users'
    }
  }
]

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(to, from, savedPosition) {
    return savedPosition ?? { left: 0, top: 0 }
  }
})

router.beforeEach((to, from, next) => {
  // if localStorage has key 'X-MiniAuth-Token' or cookie .AspNetCore.Identity.Application then continue else redirect to login.html page
  // if (localStorage.getItem('X-MiniAuth-Token') ) {
  //   next()
  // } else {
  //   window.location.href = 'login.html'
  // }
  next()
})

export default router

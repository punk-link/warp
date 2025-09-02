import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/HomeView.vue'
import Preview from '../views/PreviewView.vue'
import Entry from '../views/EntryView.vue'
import ErrorPage from '../views/ErrorView.vue'
import Deleted from '../views/DeletedView.vue'
import Privacy from '../views/Privacy.vue'

const router = createRouter({
  history: createWebHistory('/app'),
  routes: [
    { path: '/', name: 'Home', component: Home },
    { path: '/preview/:id?', name: 'Preview', component: Preview, props: true },
    { path: '/entry/:id', name: 'Entry', component: Entry, props: true },
    { path: '/error', name: 'Error', component: ErrorPage },
    { path: '/deleted', name: 'Deleted', component: Deleted },
    { path: '/privacy', name: 'Privacy', component: Privacy },
    { path: '/:pathMatch(.*)*', name: 'NotFound', component: ErrorPage }
  ]
})

export default router

import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/HomeView.vue'
import Preview from '../views/PreviewView.vue'
import Entry from '../views/EntryView.vue'
import ErrorPage from '../views/ErrorView.vue'
import Deleted from '../views/DeletedView.vue'
import Privacy from '../views/Privacy.vue'
import DataRequest from '../views/DataRequestView.vue'

const router = createRouter({
    history: createWebHistory('/'),
    routes: [
        { path: '/', name: 'Home', component: Home },
        { path: '/preview/:id?', name: 'Preview', component: Preview, props: true },
        { path: '/entry/:id', name: 'Entry', component: Entry, props: true },
        { path: '/error', name: 'Error', component: ErrorPage, meta: { pageBg: 'bg-gray-300' } },
        { path: '/deleted', name: 'Deleted', component: Deleted },
        { path: '/privacy', name: 'Privacy', component: Privacy },
        { path: '/data-request', name: 'DataRequest', component: DataRequest },
        { path: '/:pathMatch(.*)*', name: 'NotFound', component: ErrorPage, meta: { pageBg: 'bg-gray-300' } }
    ]
})

export default router

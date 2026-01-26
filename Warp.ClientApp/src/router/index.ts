import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/HomeView.vue'
import Preview from '../views/PreviewView.vue'
import Entry from '../views/EntryView.vue'
import ErrorPage from '../views/ErrorView.vue'
import Deleted from '../views/DeletedView.vue'
import Privacy from '../views/Privacy.vue'
import DataRequest from '../views/DataRequestView.vue'
import { ViewNames } from './enums/view-names'


const router = createRouter({
    history: createWebHistory('/'),
    routes: [
        { path: '/', name: ViewNames.Home, component: Home },
        { path: '/preview/:id?', name: ViewNames.Preview, component: Preview, props: true },
        { path: '/entry/:id', name: ViewNames.Entry, component: Entry, props: true },
        { path: '/error', name: ViewNames.Error, component: ErrorPage, meta: { pageBg: 'bg-gray-300' } },
        { path: '/deleted', name: ViewNames.Deleted, component: Deleted },
        { path: '/privacy', name: ViewNames.Privacy, component: Privacy },
        { path: '/data-request', name: ViewNames.DataRequest, component: DataRequest },
        { path: '/:pathMatch(.*)*', name: ViewNames.NotFound, component: ErrorPage, meta: { pageBg: 'bg-gray-300' } }
    ]
})


/** The router instance for the application. */
export default router

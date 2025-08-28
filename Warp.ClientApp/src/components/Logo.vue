<template>
    <div class="flex">
        <h1 class="logo-style">
            <RouterLink to="/" class="text-xl" @click.prevent="onLogoClick">Warplyn</RouterLink>
        </h1>
        <span class="px-2 text-gray-300 text-xs">beta</span>
    </div>
</template>

<script setup lang="ts">
import { useDraftEntry } from '../composables/useDraftEntry'
import { useRouter } from 'vue-router'


const { clearDraft } = useDraftEntry()
const router = useRouter()


function onLogoClick() {
    clearDraft()
    router.push({ name: 'Home' })
}
</script>

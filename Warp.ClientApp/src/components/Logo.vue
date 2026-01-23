<template>
    <div class="flex">
        <h1 class="logo-style">
            <RouterLink to="/" class="text-xl" @click.prevent="onLogoClick">{{ t('components.logo.title') }}</RouterLink>
        </h1>
        <span class="px-2 text-gray-300 text-xs">{{ t('components.logo.beta') }}</span>
    </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useDraftEntry } from '../composables/use-draft-entry'
import { useGallery } from '../composables/use-gallery'
import { useI18n } from 'vue-i18n'
import { ViewNames } from '../router/enums/view-names'


const { clearDraft } = useDraftEntry()
const { clear: clearGallery } = useGallery()
const router = useRouter()
const { t } = useI18n()


function onLogoClick() {
    clearDraft()
    clearGallery()
    
    router.push({ name: ViewNames.Home })
}
</script>

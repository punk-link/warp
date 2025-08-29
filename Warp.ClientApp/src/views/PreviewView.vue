<template>
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
            </div>
            <div class="service-message-container w-full sm:w-1/2 flex justify-end md:justify-center">
                <span v-if="copied && saved" class="text-secondary font-semibold text-base">link copied</span>
            </div>
        </div>

        <section class="px-3 my-5">
            <div class="flex flex-col items-center justify-around min-h-[75vh]">
                <article class="w-full md:w-1/2 bg-yellow-50 p-3 rounded-sm mb-10">
                    <div class="relative min-h-[200px]">
                        <div class="absolute -top-6 right-3 z-10" v-if="!loading && !error && !saving && !saved">
                            <OutlinePrimaryRoundButton title="Edit" @click="onEdit" icon-class="icofont-pencil-alt-2 text-xl" />
                        </div>

                        <div v-if="loading || saving" class="p-5 text-center text-gray-400">{{ loading ? 'loading...' : 'saving...' }}</div>
                        <div v-else-if="error" class="p-5 text-center text-red-500">failed to load entry</div>
                        <div v-if="images.length" class="gallery pt-5 grid grid-cols-3 gap-2">
                            <GalleryItem  v-for="(img, idx) in images" :key="idx" :id="`preview-${idx}`" :src="img" :editable="false" />
                        </div>
                        <div class="text-content font-sans-serif text-base pt-5 whitespace-pre-wrap break-words" :class="{ visible: showContent }">{{ text }}</div>
                    </div>
                </article>

                <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <template v-if="!saved">
                        <div></div>
                        <div class="bg-white rounded-sm">
                            <PrimaryButton :disabled="saving" @click="onSave" label="Save" />
                        </div>
                    </template>
                    <template v-else>
                        <div class="bg-white rounded-sm">
                            <OutlineGrayButton title="Delete" :disabled="deleting" @click="onDelete" icon-class="icofont-bin text-xl" />
                        </div>
                        <div class="bg-white rounded-sm">
                            <OutlineGrayButton title="Clone & Edit" :disabled="deleting" @click="onCloneEdit" icon-class="icofont-loop text-xl" />
                        </div>
                        <div class="bg-white rounded-sm">
                            <PrimaryButton :disabled="deleting" @click="onCopyLink" label="Copy Link" icon-class="icofont-link text-white/50" />
                        </div>
                    </template>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { entryApi } from '../api/entryApi'
import Logo from '../components/Logo.vue'
import { useDraftEntry } from '../composables/useDraftEntry'
import { useGallery } from '../composables/useGallery'
import type { DraftEntry } from '../types/draft-entry'
import PrimaryButton from '../components/PrimaryButton.vue'
import OutlineGrayButton from '../components/OutlineGrayButton.vue'
import OutlinePrimaryRoundButton from '../components/OutlinePrimaryRoundButton.vue'
import GalleryItem from '../components/GalleryItem.vue'

const route = useRoute()
const router = useRouter()
const { draft, clearDraft } = useDraftEntry()

const routeId = route.params.id as string | undefined
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)
const error = ref(false)
const text = ref('')
const images = ref<string[]>([])
const entryIdRef = ref<string | null>(null)
const { items: galleryItems, clear: clearGallery } = useGallery(entryIdRef)
const copied = ref(false)
const saved = ref<boolean>(!!routeId)
const showContent = ref(false)

// Mirror gallery item urls
watchEffect(() => {
    images.value = galleryItems.value.map(g => g.url)
})


function loadDraft(): string {
    const entry = draft.value as DraftEntry | null
    if (!entry)
        return ''

    text.value = entry.textContent || ''
    if (images.value.length === 0 && entry.images && entry.images.length) 
        images.value = [...entry.images]
    
    return entry.id || ''
}


async function onCopyLink() {
    try {
        const link = `${window.location.origin}/entry/${encodeURIComponent(routeId!)}`
        await navigator.clipboard.writeText(link)
        copied.value = true
        setTimeout(() => copied.value = false, 2500)
    } catch (e) {
        console.error('copy failed', e)
    }
}


async function onDelete() {
    if (!routeId || deleting.value) 
        return

    try {
        deleting.value = true
        
        await entryApi.deleteEntry(routeId)
        router.replace({ name: 'Deleted' })
    } catch (e) {
        console.error(e)
    } finally {
        deleting.value = false
    }
}


function onEdit() {
    router.push({ 
        name: 'Home', 
        query: draft.value?.id ? { id: draft.value.id } : {} 
    })
}


async function onSave() {
    if (saving.value || !draft.value)
        return

    try {
        saving.value = true
        const entry = draft.value
        const entryId = entry.id
        if (!entryId) 
            return

        const response: any = await entryApi.addOrUpdateEntry(entryId, {
            editMode: entry.editMode,
            expirationPeriod: entry.expirationPeriod,
            textContent: entry.textContent,
            imageIds: [] // handled server-side from uploaded files
        }, galleryItems.value.map(g => g.file))
        
        clearDraft()
        clearGallery()
        
        saved.value = true
    } catch (e) {
        console.error(e)
    } finally {
        saving.value = false
    }
}


function onCloneEdit() {
    if (!routeId) 
        return

    // load existing text into draft for editing
    setTimeout(() => { // ensure navigation after draft set
        router.push({ name: 'Home', query: { id: routeId } })
    })
}


onMounted(() => {
    const entryId = loadDraft()
    if (!entryId) {
        router.replace({ name: 'Home' })
        return
    }

    entryIdRef.value = entryId
    requestAnimationFrame(() => { showContent.value = true })
})
</script>

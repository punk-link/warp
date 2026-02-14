<template>
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
            </div>
            <div class="service-message-container w-full sm:w-1/2 flex justify-end md:justify-center">
                <span v-if="copied && saved" class="text-secondary font-semibold text-base">{{ t('entry.linkCopied') }}</span>
            </div>
        </div>

        <section class="px-3 my-5">
            <div class="flex flex-col items-center justify-around min-h-[75vh]">
                <article class="w-full md:w-1/2 bg-yellow-50 p-3 rounded-sm mb-10">
                    <div class="relative min-h-[200px]">
                        <div class="absolute -top-6 right-3 z-10" v-if="!loading && !error && !saving && !saved">
                            <Button variant="outline-primary-round" :title="t('app.actions.edit')" @click="onEdit" icon-class="icofont-pencil-alt-2 text-xl" />
                        </div>

                        <div v-if="loading || saving" class="p-5 text-center text-gray-400">
                            <div class="flex flex-col items-center justify-center">
                                <span class="loading-spinner w-5 h-5 mb-0" aria-hidden="true"></span>
                                <span class="loading-text">{{ loading ? t('entry.loading') : t('preview.saving') }}</span>
                            </div>
                        </div>
                        <div v-else-if="error" class="p-5 text-center text-red-500">
                            {{ t('preview.failedToLoad') }}
                        </div>
                        <div v-if="images.length" class="gallery pt-5 grid grid-cols-3 gap-2">
                            <GalleryItem v-for="(img, idx) in images" :key="idx" :id="`preview-${idx}`" :src="img" :editable="false" />
                        </div>
                        <div class="text-content font-sans-serif text-base pt-5 whitespace-pre-wrap break-words" :class="{ visible: showContent }" v-if="editMode === EditMode.Simple">
                            {{ text }}
                        </div>
                        <div class="text-content rich-text-content font-sans-serif text-base pt-5" :class="{ visible: showContent }" v-else-if="editMode === EditMode.Advanced" v-html="sanitizedHtml"></div>
                    </div>
                </article>

                <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <template v-if="!saved">
                        <div></div>
                        <div class="bg-white rounded-sm">
                            <Button variant="primary" :disabled="saving" :pending="saving" @click="onSave" :label="t('preview.actions.save')" />
                        </div>
                    </template>
                    <template v-else>
                        <div class="bg-white rounded-sm">
                            <Button variant="outline-gray" :title="t('preview.actions.delete')" :disabled="deleting" :pending="deleting" @click="onDelete" icon-class="icofont-bin text-xl" />
                        </div>
                        <div class="bg-white rounded-sm">
                            <Button variant="outline-gray" :title="t('preview.actions.cloneEdit')" :disabled="deleting || cloning" :pending="cloning" @click="onCloneEdit" icon-class="icofont-loop text-xl" />
                        </div>
                        <div class="bg-white rounded-sm">
                            <Button variant="primary" :disabled="deleting || copying" :pending="copying" @click="onCopyLink" :label="t('preview.actions.copyLink')" icon-class="icofont-link text-white/50" />
                        </div>
                    </template>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watchEffect, onBeforeUnmount, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router'
import { entryApi } from '../api/entry-api'
import { sanitize } from '../helpers/sanitize-html'
import Logo from '../components/Logo.vue'
import { useDraftEntry } from '../composables/use-draft-entry'
import { useGallery } from '../composables/use-gallery'
import { EditMode } from '../types/entries/enums/edit-modes'
import type { DraftEntry } from '../types/entries/draft-entry'
import Button from '../components/Button.vue'
import GalleryItem from '../components/GalleryItem.vue'
import { ViewNames } from '../router/enums/view-names'

const route = useRoute()
const router = useRouter()
const { draft, clearDraft } = useDraftEntry()

const routeId = route.params.id as string | undefined
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)
const cloning = ref(false)
const copying = ref(false)
const error = ref(false)
const text = ref('')
const contentDelta = ref('')
const editMode = ref<EditMode>(EditMode.Simple)
const images = ref<string[]>([])
const entryIdRef = ref<string | null>(null)
const { items: galleryItems, clear: clearGallery, setServerImages } = useGallery(entryIdRef)
const copied = ref(false)
const saved = ref<boolean>(!!routeId)
const preserveGalleryOnUnmount = ref(false)
const showContent = ref(false)
const { t } = useI18n()


const sanitizedHtml = computed(() => {
    if (!text.value)
        return ''
    
    return sanitize(text.value)
})


async function onCloneEdit() {
    if (!entryIdRef.value || cloning.value)
        return

    try {
        cloning.value = true
        const clone = await entryApi.copyEntry(entryIdRef.value)
        setTimeout(() => {
            router.push({ name: ViewNames.Home, query: { id: clone.id } })
        })
    } catch (e) {
        console.error('clone failed', e)
    } finally {
        cloning.value = false
    }
}


async function onCopyLink() {
    if (copying.value)
        return

    try {
        copying.value = true
        const link = `${window.location.origin}/entry/${encodeURIComponent(entryIdRef.value!)}`
        await navigator.clipboard.writeText(link)
        copied.value = true
        setTimeout(() => copied.value = false, 2500)
    } catch (e) {
        console.error('copy failed', e)
    } finally {
        copying.value = false
    }
}


async function onDelete() {
    if (!entryIdRef.value || deleting.value)
        return

    try {
        deleting.value = true

        await entryApi.deleteEntry(entryIdRef.value)
        router.replace({ name: ViewNames.Deleted })
    } catch (e) {
        console.error(e)
    } finally {
        deleting.value = false
    }
}


function onEdit() {
    preserveGalleryOnUnmount.value = true

    router.push({
        name: ViewNames.Home,
        query: draft.value?.id ? { id: draft.value.id } : {}
    })
}


async function onSave() {
    if (saving.value || !draft.value)
        return

    const entryId = draft.value.id
    if (!entryId)
        return

    try {
        saving.value = true
        
        await saveEntry(entryId)
        await refreshServerImages(entryId)
        
        clearDraft()
        saved.value = true
    } catch (e) {
        console.error(e)
    } finally {
        saving.value = false
    }
}


async function saveEntry(entryId: string) {
    const entry = draft.value!
    const localFiles = galleryItems.value
        .filter((g: any) => g.kind === 'local')
        .map((g: any) => g.file)

    const imageIds = galleryItems.value
        .filter((g: any) => g.kind === 'remote')
        .map((g: any) => extractImageIdFromUrl(g.url))
        .filter((id: string | null) => !!id)

    await entryApi.addOrUpdateEntry(entryId, {
        editMode: entry.editMode,
        expirationPeriod: entry.expirationPeriod,
        textContent: entry.textContent,
        contentDelta: entry.contentDelta,
        imageIds: imageIds
    }, localFiles)
}


function extractImageIdFromUrl(url: string): string | null {
    try {
        const match = url.match(/\/image-id\/([^/]+)/)
        return match ? match[1] : null
    } catch {
        return null
    }
}


async function refreshServerImages(entryId: string) {
    try {
        const savedEntry: any = await entryApi.getEntry(entryId)
        const serverUrls = extractImageUrls(savedEntry)

        if (serverUrls.length) {
            setServerImages(serverUrls)
            images.value = [...serverUrls]
        }
    } catch (e) {
        console.error('failed to rehydrate server image urls', e)
    }
}


function extractImageUrls(savedEntry: any): string[] {
    if (!Array.isArray(savedEntry?.images))
        return []

    return savedEntry.images
        .map((i: any) => typeof i === 'string' ? i : (i?.url ?? ''))
        .filter((u: string) => !!u)
}


onBeforeUnmount(() => {
    if (!preserveGalleryOnUnmount.value)
        clearGallery()
})


onBeforeRouteLeave((to) => {
    if (to.name === ViewNames.Home)
        preserveGalleryOnUnmount.value = true
})


onMounted(async () => {
    const entry = draft.value as DraftEntry | null
    if (!entry?.id) {
        router.replace({ name: ViewNames.Home })
        return
    }

    entryIdRef.value = entry.id
    text.value = entry.textContent || ''
    editMode.value = entry.editMode
    
    if (entry.contentDelta)
        contentDelta.value = entry.contentDelta

    await nextTick()

    if (entry.images && entry.images.length > 0) {
        const remoteUrls = entry.images.filter(url => url.startsWith('/api/'))
        if (remoteUrls.length > 0 && galleryItems.value.length === 0)
            setServerImages(remoteUrls)
    }

    requestAnimationFrame(() => { showContent.value = true })
})


// Mirror gallery item urls
watchEffect(() => {
    images.value = galleryItems.value.map(g => g.url)
})
</script>

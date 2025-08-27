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
                <!-- Content -->
                <article class="w-full md:w-1/2 bg-yellow-50 p-3 rounded-sm mb-10">
                    <div class="relative min-h-[200px]">
                        <div class="absolute -top-6 right-3 z-10" v-if="!loading && !error && !saving && !saved">
                            <button class="btn btn-round btn-outline-primary" title="Edit" @click="onEdit">
                                <i class="icofont-pencil-alt-2 text-xl" />
                            </button>
                        </div>

                        <div v-if="loading || saving" class="p-5 text-center text-gray-400">{{ loading ? 'loading...' : 'saving...' }}</div>
                        <div v-else-if="error" class="p-5 text-center text-red-500">failed to load entry</div>
                        <div v-if="images.length" class="gallery pt-5 grid grid-cols-3 gap-2">
                            <img v-for="(img, idx) in images" :key="idx" :src="img" class="rounded shadow-sm object-cover max-h-40" />
                        </div>
                        <div class="text-content font-sans-serif text-base pt-5 whitespace-pre-wrap break-words" :class="{ visible: showContent }">{{ text }}</div>
                    </div>
                </article>

                <!-- Action Buttons -->
                <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <template v-if="!saved">
                        <div class="bg-white rounded-sm">
                            <button class="btn btn-outline-gray" title="Cancel" :disabled="saving" @click="onCancel">
                                <i class="icofont-close text-xl" />
                            </button>
                        </div>
                        <div class="bg-white rounded-sm">
                            <button class="btn btn-outline-gray" title="Back & edit" :disabled="saving" @click="onEdit">
                                <i class="icofont-pencil-alt-2 text-xl" />
                            </button>
                        </div>
                        <div class="bg-white rounded-sm">
                            <button class="btn btn-primary" title="Save" :disabled="saving" @click="onSave">
                                <i class="icofont-save text-white/50" />
                                Save
                            </button>
                        </div>
                    </template>
                    <template v-else>
                        <div class="bg-white rounded-sm">
                            <button class="btn btn-outline-gray" title="Delete" :disabled="deleting" @click="onDelete">
                                <i class="icofont-bin text-xl" />
                            </button>
                        </div>
                        <div class="bg-white rounded-sm">
                            <button class="btn btn-outline-gray" title="Clone & edit" :disabled="deleting" @click="onCloneEdit">
                                <i class="icofont-loop text-xl" />
                            </button>
                        </div>
                        <div class="bg-white rounded-sm">
                            <button class="btn btn-primary" title="Copy link" :disabled="deleting" @click="onCopy">
                                <i class="icofont-link text-white/50" />
                                Copy link
                            </button>
                        </div>
                    </template>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { entryApi } from '../api/entryApi'
import Logo from '../components/Logo.vue'
import { useDraftEntry } from '../composables/useDraftEntry'
import { uploadImages } from '../composables/useImageUpload'
import type { DraftEntry } from '../types/draft-entry'
import { ExpirationPeriod, parseExpirationPeriod } from '../types/expiration-periods'
import { EditMode } from '../types/edit-modes'

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
const files = ref<File[]>([])
const expiration = ref<ExpirationPeriod | null>(null)
const mode = ref<EditMode>(EditMode.Simple)
const copied = ref(false)
const saved = ref<boolean>(!!routeId)
const showContent = ref(false)
let objectUrls: string[] = []

function buildObjectUrls(fs: File[]) {
    cleanupObjectUrls()
    for (const f of fs) {
        const url = URL.createObjectURL(f)
        objectUrls.push(url)
    }
    images.value = [...objectUrls]
}

function cleanupObjectUrls() {
    for (const u of objectUrls) {
        try { URL.revokeObjectURL(u) } catch {}
    }
    objectUrls = []
}

onBeforeUnmount(cleanupObjectUrls)

function loadDraft(): string {
    const d = draft.value as DraftEntry | null
    if (!d)
        return ''
    text.value = d.textContent || ''
    const raw = (d as any).expirationPeriod
    expiration.value = parseExpirationPeriod(raw)
    return d.id || ''
}

function onEdit() {
    // Navigate back to home with draft intact
    router.push({ name: 'Home', query: draft.value?.id ? { id: draft.value.id } : {} })
}

async function onSave() {
    if (saving.value) return
    try {
        saving.value = true
        const form = new FormData()
        form.append('textContent', text.value)
    form.append('expiration', expiration.value != null ? expiration.value : '')
    form.append('editMode', mode.value)
        let id = draft.value?.id
        if (id) {
            await entryApi.updateEntry(id, form)
        } else {
            const resp: any = await entryApi.createEntry(form)
            id = resp.id
        }
        if (files.value.length > 0 && id) {
            await uploadImages(id, files.value)
        }
        clearDraft()
        saved.value = true
        router.replace({ name: 'Preview', params: { id } })
    } catch (e) {
        console.error(e)
    } finally {
        saving.value = false
    }
}

function onCancel() {
    clearDraft()
    router.push({ name: 'Home' })
}

async function onDelete() {
    if (!routeId || deleting.value) return
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

function onCloneEdit() {
    if (!routeId) return
    // load existing text into draft for editing
    setTimeout(() => { // ensure navigation after draft set
        router.push({ name: 'Home', query: { id: routeId } })
    })
}

async function onCopy() {
    try {
        const link = `${window.location.origin}/entry/${encodeURIComponent(routeId!)}`
        await navigator.clipboard.writeText(link)
        copied.value = true
        setTimeout(() => copied.value = false, 2500)
    } catch (e) {
        console.error('copy failed', e)
    }
}

onMounted(() => {
    const entryId = loadDraft()
    if (!entryId) {
        router.replace({ name: 'Home' })
        return
    }
    
    requestAnimationFrame(() => { showContent.value = true })
    router.replace({ query: { ...route.query, id: entryId } })
})
</script>


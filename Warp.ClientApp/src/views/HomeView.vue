<template>
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
            </div>
            <div class="w-full sm:w-1/2 flex justify-end md:justify-center">
                <ModeSwitch v-model="mode" :disabled="pending" simple-label="Text" advanced-label="Advanced" />
            </div>
        </div>

        <section class="px-3 my-5">
            <div class="flex flex-col items-center justify-around min-h-[75vh]">

                <div v-if="mode === EditMode.Advanced" class="w-full">
                    <AdvancedEditor>
                        <template #text>
                            <DynamicTextArea v-model="text" label="Text" />
                        </template>
                        <template #gallery>
                            <div class="gallery" ref="dropAreaRef">
                                <GalleryItem v-for="(it, idx) in items" :key="idx" :id="`file-${idx}`" :src="it.url"
                                    :name="it.file.name" @remove="() => removeItem(idx)" />
                                <GalleryItem id="empty" @click="() => fileInputRef?.click()" />
                                <input ref="fileInputRef" type="file" class="hidden" multiple accept="image/*"
                                    @change="onFileInputChange" />
                            </div>
                        </template>
                    </AdvancedEditor>
                </div>

                <div v-else class="w-full">
                    <SimpleEditor>
                        <template #text>
                            <DynamicTextArea v-model="text" placeholder="Type or paste your text here" />
                        </template>
                    </SimpleEditor>
                </div>

                <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <div class="bg-white rounded-sm p-2">
                        <ExpirationSelect v-model="expiration" :options="expirationOptions" label="Expires in" />
                    </div>
                    <div class="bg-white rounded-sm p-2">
                        <PrimaryButton :disabled="!isValid" :pending="pending" @click="onPreview" label="Preview" />
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { EditMode, parseEditMode } from '../types/edit-modes'
import { useGallery } from '../composables/useGallery'
import { initDropAreaHandlers, handlePaste, uploadImages } from '../composables/useImageUpload'
import { useRoute, useRouter } from 'vue-router'
import { entryApi } from '../api/entryApi'
import { creatorApi } from '../api/creatorApi'
import { useMetadata } from '../composables/useMetadata'
import Logo from '../components/Logo.vue'
import ModeSwitch from '../components/ModeSwitch.vue'
import SimpleEditor from '../components/SimpleEditor.vue'
import DynamicTextArea from '../components/DynamicTextArea.vue'
import AdvancedEditor from '../components/AdvancedEditor.vue'
import GalleryItem from '../components/GalleryItem.vue'
import ExpirationSelect from '../components/ExpirationSelect.vue'
import PrimaryButton from '../components/PrimaryButton.vue'
import { useDraftEntry } from '../composables/useDraftEntry'
import { DraftEntry } from '../types/draft-entry'
import { ExpirationPeriod } from '../types/expiration-periods'

const EDIT_MODE_STORAGE_KEY = 'warp.editMode'
const mode = ref<EditMode>(EditMode.Simple)
const text = ref<string>('')
const expiration = ref<ExpirationPeriod>(ExpirationPeriod.FiveMinutes)
const expirationOptions = ref<ExpirationPeriod[]>([])
const pending = ref(false)
const entryIdRef = ref<string | null>(null)

const dropAreaRef = ref<HTMLElement | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)
const { items, addFiles, remove, count: galleryCount } = useGallery(entryIdRef)

const { setDraft, draft } = useDraftEntry()
const route = useRoute()
const router = useRouter()
const metadata = useMetadata()

// Collect cleanup callbacks so we can register a single lifecycle hook during setup.
const cleanupFns: Array<() => void> = []
onBeforeUnmount(() => {
    for (const fn of cleanupFns) {
        try { fn() } catch { /* noop */ }
    }
})


function removeItem(idx: number) {
    remove(idx)
}


const isValid = computed(() => text.value.trim().length > 0 || galleryCount.value > 0)


function getEditMode(editMode: EditMode): EditMode {
    if (editMode !== EditMode.Unset)
        return editMode

    const stored = localStorage.getItem(EDIT_MODE_STORAGE_KEY)
    if (stored) 
        return parseEditMode(stored)

    return EditMode.Simple
}


function onFilesSelected(e: Event) {
    const input = e.target as HTMLInputElement
    if (!input.files) return
    addFiles(input.files)
    input.value = ''
}


function onFileInputChange(e: Event) {
    const input = e.target as HTMLInputElement
    if (!input.files) return
    addFiles(input.files)
    input.value = ''
}


function hydrateStateFromDraft(draft: DraftEntry): string {
    entryIdRef.value = draft.id

    mode.value = getEditMode(parseEditMode(draft.editMode as unknown))
    expiration.value = draft.expirationPeriod ?? ExpirationPeriod.FiveMinutes
    text.value = draft.textContent

    return draft.id
}


async function initiateStateFromServer(): Promise<string> {
    await creatorApi.getOrSetCreator()

    let existingId = (route.query?.id as string | undefined) ?? undefined
    let entry = existingId === undefined
        ? await entryApi.getEntry()
        : await entryApi.getEntry(existingId)

    entryIdRef.value = entry.id

    entry.editMode = getEditMode(parseEditMode(entry.editMode as unknown))
    mode.value = entry.editMode
    expiration.value = entry.expirationPeriod ?? ExpirationPeriod.FiveMinutes
    text.value = entry.textContent

    return entry.id
}


function onPreview() {
    if (!isValid.value || pending.value)
        return
    
    setDraft({
        id: entryIdRef.value!,
        editMode: mode.value,
        expirationPeriod: expiration.value!,
        images: items.value.map(i => i.url),
        textContent: text.value
    })

    router.push({ 
        name: 'Preview', 
        query: { id: entryIdRef.value }
    })
}


onMounted(async () => {
    try {
        pending.value = true

        expirationOptions.value = await metadata.loadExpirationOptions()

        let entryId: string | null = null
        if (draft.value) 
            entryId = hydrateStateFromDraft(draft.value)
        else 
            entryId = await initiateStateFromServer()

        if (!route.query.id && entryId) 
            router.replace({ query: { ...route.query, id: entryId } })
        
        if (dropAreaRef.value && fileInputRef.value) {
            const cleanup = initDropAreaHandlers(dropAreaRef.value, fileInputRef.value, () => (route.query.id as string | undefined) ?? undefined)
            if (typeof cleanup === 'function')
                cleanupFns.push(cleanup)
        }

        const pasteHandler = (e: ClipboardEvent) => void handlePaste(e, () => (route.query.id as string | undefined) ?? undefined)
        window.addEventListener('paste', pasteHandler as EventListener)
        cleanupFns.push(() => window.removeEventListener('paste', pasteHandler as EventListener))
    } catch {
        router.replace({ name: 'Error' })
    } finally {
        pending.value = false
    }
})


watch(mode, (val) => {
    localStorage.setItem(EDIT_MODE_STORAGE_KEY, val)
})
</script>

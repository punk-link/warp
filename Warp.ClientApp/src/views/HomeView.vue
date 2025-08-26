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
import { EditMode } from '../types/edit-modes'
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
const files = ref<File[]>([])
const expiration = ref<ExpirationPeriod>(ExpirationPeriod.FiveMinutes)
const expirationOptions = ref<ExpirationPeriod[]>([])
const pending = ref(false)
const entryIdRef = ref<string | null>(null)


const dropAreaRef = ref<HTMLElement | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)
const { items, addFiles, remove } = useGallery()

const { setDraft, draft } = useDraftEntry()
const route = useRoute()
const router = useRouter()
const metadata = useMetadata()


function removeItem(idx: number) {
    remove(idx)
}


function coerceExpiration(val: unknown): ExpirationPeriod {
    if (typeof val === 'number') return val as ExpirationPeriod
    if (typeof val === 'string') {
        const n = Number(val)
        if (!Number.isNaN(n)) return n as ExpirationPeriod
    }
    return ExpirationPeriod.FiveMinutes
}


const isValid = computed(() => text.value.trim().length > 0 || files.value.length > 0)


function getInitialEditMode(mode: EditMode): EditMode {
    if (mode !== EditMode.Unset)
        return mode

    const stored = localStorage.getItem(EDIT_MODE_STORAGE_KEY)
    const num = stored != null ? Number(stored) : NaN
    if (!Number.isNaN(num) && (num in EditMode))
        return num as EditMode

    return EditMode.Simple
}


function onFilesSelected(e: Event) {
    const input = e.target as HTMLInputElement
    if (!input.files) return
    files.value = [...files.value, ...Array.from(input.files)]
    input.value = ''
}


function onFileInputChange(e: Event) {
    const input = e.target as HTMLInputElement
    if (!input.files)
        return

    addFiles(input.files)

    files.value = [...files.value, ...Array.from(input.files)]
    input.value = ''
}


function hydrateStateFromDraft(draft: DraftEntry): string {
    entryIdRef.value = draft.id

    mode.value = getInitialEditMode(draft.editMode)
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

    entry.editMode = getInitialEditMode(entry.editMode)
    mode.value = entry.editMode
    expiration.value = entry.expirationPeriod ?? ExpirationPeriod.FiveMinutes

    text.value = entry.textContent

    return entry.id
}


function onPreview() {
    if (!isValid.value || pending.value)
        return
    
    setDraft({
        id: entryIdRef.value,
        editMode: mode.value,
    expirationPeriod: expiration.value!,
        images: [],
        textContent: text.value
    } as DraftEntry)

    router.push({ name: 'Preview' })
}


onMounted(async () => {
    try {
        pending.value = true

        expirationOptions.value = (await metadata.loadExpirationOptions())
            .map(option => coerceExpiration(option))

        let entryId: string | null = null
        if (draft.value) 
            entryId = hydrateStateFromDraft(draft.value)
        else 
            entryId = await initiateStateFromServer()

        if (!route.query.id && entryId) 
            router.replace({ query: { ...route.query, id: entryId } })
        
        if (dropAreaRef.value && fileInputRef.value) {
            const cleanup = initDropAreaHandlers(dropAreaRef.value, fileInputRef.value, () => (route.query.id as string | undefined) ?? undefined)
            if (cleanup && typeof cleanup === 'function')
                onBeforeUnmount(() => cleanup())
        }

        const pasteHandler = (e: ClipboardEvent) => void handlePaste(e, () => (route.query.id as string | undefined) ?? undefined)
        window.addEventListener('paste', pasteHandler as EventListener)
        onBeforeUnmount(() => window.removeEventListener('paste', pasteHandler as EventListener))
    } catch {
        router.replace({ name: 'Error' })
    } finally {
        pending.value = false
    }
})


watch(mode, (val) => {
    try {
        localStorage.setItem(EDIT_MODE_STORAGE_KEY, String(val))
    } catch {
        throw new Error('Failed to save edit mode')
    }
})
</script>

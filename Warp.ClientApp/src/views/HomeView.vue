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

        <div v-if="mode === 'Advanced'" class="w-full">
          <AdvancedEditor>
            <template #text>
              <DynamicTextArea v-model="text" label="Text" />
            </template>
            <template #gallery>
              <div class="gallery" ref="dropAreaRef">
                <GalleryItem
                  v-for="(it, idx) in items"
                  :key="idx"
                  :id="`file-${idx}`"
                  :src="it.url"
                  :name="it.file.name"
                  @remove="() => removeItem(idx)"
                />
                <GalleryItem id="empty" @click="() => fileInputRef?.click()" />
                <input ref="fileInputRef" type="file" class="hidden" multiple accept="image/*" @change="onFileInputChange" />
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
            <CreateButton :disabled="!isValid" :pending="pending" @click="onCreate" />
          </div>
        </div>
      </div>
    </section>
  </div>
  
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import type { EditMode } from '../types/edit-mode'
import type { ExpirationOption } from '../types/expiration'
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
import CreateButton from '../components/CreateButton.vue'
import { useDraftEntry } from '../composables/useDraftEntry'
import { DraftEntry } from '../types/draft-entry'

const EDIT_MODE_STORAGE_KEY = 'warp.editMode'
const mode = ref<EditMode>('Simple')
const text = ref<string>('')
const files = ref<File[]>([])
const expiration = ref<string | null>(null)
const expirationOptions = ref<ExpirationOption[]>([])
const pending = ref(false)
const entryIdRef = ref<string | null>(null)


const dropAreaRef = ref<HTMLElement | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)
const { items, addFiles, remove } = useGallery()

const { setDraft } = useDraftEntry()
const route = useRoute()
const router = useRouter()
const metadata = useMetadata()


function removeItem(idx: number) {
  remove(idx)
}


function getInitialEditMode(mode: EditMode): EditMode {
  if (mode !== 'Unset')
    return mode

  const stored = localStorage.getItem(EDIT_MODE_STORAGE_KEY)
  if (stored === 'Simple' || stored === 'Advanced')
    return stored as EditMode
  
  return 'Simple' as EditMode
}


const isValid = computed(() => text.value.trim().length > 0 || files.value.length > 0)


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

function onCreate() {
  if (!isValid.value || pending.value) 
    return

  const entryId = entryIdRef.value
  if (!entryId)
    return
  setDraft({
    id: entryId,
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

    const opts = await metadata.loadExpirationOptions()
    expirationOptions.value = opts as ExpirationOption[]

    await creatorApi.getOrSetCreator()

    let existingId = (route.query?.id as string | undefined) ?? undefined
    let entry = existingId === undefined
      ? await entryApi.getEntry()
      : await entryApi.getEntry(existingId)

    entryIdRef.value = entry.id
    if (!existingId) 
      router.replace({ query: { ...route.query, id: entry.id } })

    entry.editMode = getInitialEditMode(entry.editMode)
    mode.value = entry.editMode

    text.value = entry.textContent

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
    localStorage.setItem(EDIT_MODE_STORAGE_KEY, val)
  } catch {
    throw new Error('Failed to save edit mode')
  }
})
</script>

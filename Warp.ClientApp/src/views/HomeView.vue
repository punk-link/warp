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
              <div class="gallery">
                <GalleryItem
                  v-for="(f, idx) in files"
                  :key="idx"
                  :id="`file-${idx}`"
                  :name="f.name"
                  @remove="removeFile(idx)"
                />
                <GalleryItem id="empty" />
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
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { EditMode } from '../types/edit-mode'
import { entryApi } from '../api/entryApi'
import { creatorApi } from '../api/creatorApi'
import { useMetadata } from '../composables/useMetadata'
import type { ExpirationOption } from '../types/expiration'
import Logo from '../components/Logo.vue'
import ModeSwitch from '../components/ModeSwitch.vue'
import SimpleEditor from '../components/SimpleEditor.vue'
import DynamicTextArea from '../components/DynamicTextArea.vue'
import AdvancedEditor from '../components/AdvancedEditor.vue'
import GalleryItem from '../components/GalleryItem.vue'
import ExpirationSelect from '../components/ExpirationSelect.vue'
import CreateButton from '../components/CreateButton.vue'

const EDIT_MODE_STORAGE_KEY = 'warp.editMode'
const mode = ref<EditMode>('Unset')
const text = ref<string>('')
const files = ref<File[]>([])
const expiration = ref<string | null>(null)
const expirationOptions = ref<ExpirationOption[]>([])
const pending = ref(false)

const route = useRoute()
const router = useRouter()
const metadata = useMetadata()

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

function removeFile(index: number) {
  files.value.splice(index, 1)
}

async function onCreate() {
  if (!isValid.value || pending.value) 
    return
  
    // TODO: integrate with useIndexForm composable (create/update with CSRF)
}

onMounted(async () => {
  pending.value = true

  const opts = await metadata.loadExpirationOptions()
  expirationOptions.value = opts as ExpirationOption[]

  await creatorApi.getOrSetCreator()

  let entryId = (route.query?.id as string | undefined) ?? undefined
  let entry = entryId === undefined
    ? await entryApi.getEntry()
    : await entryApi.getEntry(entryId)

  try {
    entry.editMode = getInitialEditMode(entry.editMode)
    mode.value = entry.editMode

    text.value = entry.textContent
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
    // ignore
  }
})
</script>

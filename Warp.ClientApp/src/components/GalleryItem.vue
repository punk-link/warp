<template>
    <div v-if="src" :id="`image-container-${id}`" :data-image-id="id"
        class="image-container overflow-visible animate-catchy-fade-in relative">
        <img :src="src" :alt="name || 'image'" @load="onLoad" @error="onError" :class="{ 'opacity-0': !loaded }" />
        <div v-if="!loaded" class="absolute inset-0 flex flex-col items-center justify-center bg-white/40">
            <span class="loading-spinner w-4 h-4 mb-0" aria-hidden="true"></span>
            <span class="loading-text sr-only">loading image</span>
        </div>
        <input v-if="editable" type="hidden" name="ImageIds" :value="id" />
        <div v-if="editable" class="absolute bottom-0 w-full flex justify-center" style="transform: translateY(50%);">
            <button @click.prevent="emitRemove" type="button" class="delete-image-button btn btn-round" :title="t('app.actions.delete')">
                <i class="icofont-bin text-xl"></i>
            </button>
        </div>
    </div>
    <div v-else id="empty-image-container"
        class="image-container flex items-center justify-center border-primary border-2">
        <i class="icofont-plus text-gray-300 text-4xl"></i>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'


interface Props {
    id: string
    src?: string
    name?: string
    editable?: boolean
}


const { t } = useI18n()


const { id, src, name, editable = true } = defineProps<Props>()
const loaded = ref(false)
const emit = defineEmits<{ (e: 'remove'): void }>()


function onLoad() { 
    loaded.value = true 
}


function onError() { 
    loaded.value = true 
}


function emitRemove() {
    emit('remove')
}
</script>

<template>
    <div class="flex flex-col">
        <label v-if="label" class="form-label floating-label order-1">{{ label }}</label>
        <textarea ref="element" class="form-textarea order-2 bg-transparent" placeholder=" "
            style="height:40px; overflow-y:hidden;" :maxlength="maxLength ?? undefined" :disabled="disabled"
            :aria-label="ariaLabel || t('home.editor.textLabel') || 'Text'" :value="modelValue" @input="onInput" />
        <label class="form-label floating-label order-1" style="z-index: -1;">{{ placeholder }}</label>
        <p v-if="helper" class="text-xs text-gray-400 mt-2">{{ helper }}</p>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'


interface Props {
    modelValue: string;
    placeholder?: string;
    helper?: string;
    maxLength?: number;
    disabled?: boolean;
    label?: string;
    ariaLabel?: string;
}


const { t } = useI18n()


const props = withDefaults(defineProps<Props>(), {
    modelValue: '',
    placeholder: '',
    helper: '',
    label: '',
    disabled: false,
});
const element = ref<HTMLTextAreaElement | null>(null)
const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()


function resize() {
    const textArea = element.value
    if (!textArea)
        return

    const minHeight = 40

    textArea.style.height = 'auto'
    const newHeight = Math.max(textArea.scrollHeight, minHeight)
    textArea.style.height = newHeight + 'px'
}


function onInput(e: Event) {
    const val = (e.target as HTMLTextAreaElement).value
    emit('update:modelValue', val)
    resize()
}


watch(() => props.modelValue, async () => {
    await nextTick()
    resize()
})


onMounted(() => resize())
</script>

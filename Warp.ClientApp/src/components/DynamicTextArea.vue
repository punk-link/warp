<template>
    <div class="flex flex-col">
        <label v-if="label" class="form-label floating-label order-1">{{ label }}</label>
        <div class="textarea-wrapper">
            <textarea ref="element" class="form-textarea order-2 bg-transparent" placeholder=" "
                style="height:40px; overflow-y:hidden;" :maxlength="maxLength ?? undefined" :disabled="disabled"
                :aria-label="ariaLabel || t('home.editor.textLabel') || 'Text'" :value="modelValue" @input="onInput" />
        </div>
        <label class="form-label floating-label order-1" style="z-index: -1;">{{ placeholder }}</label>
        <div v-if="showWarning" class="flex items-center justify-end gap-2 mt-1">
            <div class="w-2 h-2 rounded-full" :class="circleColor"></div>
            <span class="text-xs text-gray-600">{{ warningText }}</span>
        </div>
        <p v-if="helper" class="text-xs text-gray-400 mt-2">{{ helper }}</p>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, nextTick, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useContentSizeIndicator, MAX_PLAIN_TEXT_SIZE } from '../composables/use-content-size-indicator'


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
const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
    (e: 'update:sizeWarning', sizeBytes: number, isOverLimit: boolean): void;
}>()


const modelValueRef = computed(() => props.modelValue)
const { sizeBytes, circleColor, warningText, showWarning, isOverLimit } = useContentSizeIndicator(modelValueRef, MAX_PLAIN_TEXT_SIZE)

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
    emit('update:sizeWarning', sizeBytes.value, isOverLimit.value)
    resize()
}


watch(() => props.modelValue, async () => {
    await nextTick()
    resize()
    emit('update:sizeWarning', sizeBytes.value, isOverLimit.value)
})


onMounted(() => {
    resize()
    emit('update:sizeWarning', sizeBytes.value, isOverLimit.value)
})
</script>

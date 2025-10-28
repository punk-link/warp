<template>
    <button :type="type" :disabled="disabled || pending" :class="computedClasses"
        :aria-busy="pending ? 'true' : 'false'" @click="onClick">
        <i v-if="!pending && iconClass" :class="iconClass" aria-hidden="true"></i>
        <span v-if="!pending && label !== null">{{ label }}</span>
        <span v-else-if="pending" class="inline-flex items-center justify-center">
            <span class="loading-spinner w-5 h-5 mb-0" aria-hidden="true"></span>
            <span class="sr-only">{{ pendingLabelComputed }}</span>
        </span>
    </button>
</template>

<script setup lang="ts">
import { computed, watch, onBeforeUnmount } from 'vue'
import { useI18n } from 'vue-i18n'


interface Props {
    variant?: 'primary' | 'secondary' | 'gray' | 'outline-gray' | 'outline-secondary' | 'outline-primary-round'
    label?: string | null
    pending?: boolean
    pendingLabel?: string
    disabled?: boolean
    iconClass?: string
    type?: 'button' | 'submit' | 'reset'
}


const props = withDefaults(defineProps<Props>(), {
    variant: 'primary',
    label: null,
    pending: false,
    pendingLabel: undefined,
    disabled: false,
    iconClass: undefined,
    type: 'button'
})


const { t } = useI18n()
const pendingLabelComputed = computed(() => props.pendingLabel ?? t('components.buttons.pending') ?? 'Pending…')
const computedClasses = computed(() => [
    variantClassMap[props.variant]
])


const emit = defineEmits<{ (e: 'click'): void }>()


function onClick() {
    if (props.disabled || props.pending) 
        return

    emit('click')
}


const variantClassMap: Record<NonNullable<Props['variant']>, string> = {
    primary: 'btn btn-primary',
    secondary: 'btn btn-secondary',
    gray: 'btn btn-gray',
    'outline-gray': 'btn btn-outline-gray',
    'outline-secondary': 'btn btn-outline-secondary',
    'outline-primary-round': 'btn btn-round btn-outline-primary'
}


let globalPendingCount = 0

function updateRootPendingClass() {
    const root = document.documentElement
    if (!root) 
        return

    if (globalPendingCount > 0)
        root.classList.add('app-pending')
    else
        root.classList.remove('app-pending')
}


watch(() => props.pending, (now, prev) => {
    if (now && !prev) {
        globalPendingCount++
        updateRootPendingClass()
    } else if (!now && prev) {
        globalPendingCount = Math.max(0, globalPendingCount - 1)
        updateRootPendingClass()
    }
}, { immediate: true })


onBeforeUnmount(() => {
    if (props.pending) {
        globalPendingCount = Math.max(0, globalPendingCount - 1)
        updateRootPendingClass()
    }
})
</script>


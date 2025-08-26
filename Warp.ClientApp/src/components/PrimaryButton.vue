<template>
    <button type="button" class="btn btn-primary inline-flex items-center justify-center gap-2 relative" :disabled="disabled || pending" @click="$emit('click')">
        <span v-if="!pending" class="inline-flex items-center gap-2">
            <i v-if="iconClass" :class="iconClass" aria-hidden="true"></i>
            <slot>{{ label }}</slot>
        </span>
        <span v-else class="inline-flex items-center justify-center">
            <span class="loading-spinner w-5 h-5 mb-0" aria-hidden="true"></span>
            <span class="sr-only">{{ pendingLabel }}</span>
        </span>
    </button>
</template>

<script setup lang="ts">
interface Props {
    disabled?: boolean;
    pending?: boolean;
    label?: string;
    pendingLabel?: string;
    iconClass?: string;
    pendingIconClass?: string;
}


const props = withDefaults(defineProps<Props>(), {
    disabled: false,
    pending: false,
    label: 'Ok',
    pendingLabel: 'Pending…',
    iconClass: undefined,
    pendingIconClass: undefined
});


defineEmits<{ (e: 'click'): void }>();
</script>

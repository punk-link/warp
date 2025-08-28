<template>
    <button type="button" class="btn btn-round btn-outline-primary" :disabled="disabled || pending" @click="$emit('click')" v-bind="$attrs">
        <i v-if="!pending && iconClass" :class="iconClass" aria-hidden="true"></i>
        <span v-if="!pending && label !== null">{{ label }}</span>
        <span v-else-if="pending" class="inline-flex items-center justify-center">
            <span class="loading-spinner w-5 h-5 mb-0" aria-hidden="true"></span>
            <span class="sr-only">{{ pendingLabel }}</span>
        </span>
    </button>
</template>

<script setup lang="ts">
interface Props {
    disabled?: boolean;
    pending?: boolean;
    label?: string | null;
    pendingLabel?: string;
    iconClass?: string;
}

withDefaults(defineProps<Props>(), {
    disabled: false,
    pending: false,
    label: null,
    pendingLabel: 'Pending…',
    iconClass: undefined
});

defineEmits<{ (e: 'click'): void }>();
</script>

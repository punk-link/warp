<template>
  <button type="button" class="btn btn-primary" :disabled="disabled || pending" @click="$emit('click')">
    <span v-if="pending" class="animate-pulse">{{ pendingLabel }}</span>
    <span v-else>{{ label }}</span>
  </button>
</template>

<script setup lang="ts">
interface Props {
  disabled?: boolean;
  pending?: boolean;
  label?: string;
  pendingLabel?: string;
}

withDefaults(defineProps<Props>(), {
  disabled: false,
  pending: false,
  label: 'Create',
  pendingLabel: 'Creating…',
});

defineEmits<{ (e: 'click'): void }>();
</script>

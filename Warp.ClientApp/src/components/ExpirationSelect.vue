<template>
  <label class="form-label">
    {{ label }}
  </label>
  <div class="flex items-baseline">
    <i class="icofont-close text-primary text-base mr-2"></i>
    <select
      class="form-select"
      :disabled="disabled"
      :value="modelValue ?? ''"
      @change="$emit('update:modelValue', ($event.target as HTMLSelectElement).value || null)"
      :aria-label="ariaLabel || label"
    >
      <option v-for="opt in options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
    </select>
  </div>
</template>

<script setup lang="ts">
export interface ExpirationOption { value: string; label: string }

interface Props {
  modelValue: string | null;
  options: ExpirationOption[];
  label?: string;
  disabled?: boolean;
  ariaLabel?: string;
}

withDefaults(defineProps<Props>(), {
  label: 'Expires in',
  disabled: false,
});

defineEmits<{
  (e: 'update:modelValue', value: string | null): void;
}>();
</script>

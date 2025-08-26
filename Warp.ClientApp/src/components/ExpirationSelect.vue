<template>
  <label class="form-label">
    {{ label }}
  </label>
  <div class="flex items-baseline">
    <i class="icofont-close text-primary text-base mr-2"></i>
    <select
      class="form-select"
      :disabled="disabled"
      :value="modelValue"
      @change="onChange"
      :aria-label="ariaLabel || label"
    >
      <option v-for="opt in options" :key="Number(opt)" :value="Number(opt)">{{ ExpirationPeriod[opt] }}</option>
    </select>
  </div>
</template>

<script setup lang="ts">
import { ExpirationPeriod } from '../types/expiration-periods'


interface Props {
  modelValue: ExpirationPeriod;
  options: ExpirationPeriod[];
  label?: string;
  disabled?: boolean;
  ariaLabel?: string;
}


withDefaults(defineProps<Props>(), {
  modelValue: ExpirationPeriod.FiveMinutes,
  label: 'Expires in',
  disabled: false,
});


const emit = defineEmits<{
  (e: 'update:modelValue', value: ExpirationPeriod): void;
}>();


function onChange(e: Event) {
  const val = (e.target as HTMLSelectElement).value
  const num = Number(val)
  emit('update:modelValue', num as ExpirationPeriod)
}
</script>

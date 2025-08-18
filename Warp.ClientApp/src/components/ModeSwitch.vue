<template>
  <div class="flex items-center gap-2">
  <button type="button" :class="['nav-button', { active: modelValue === 'simple' }]" :disabled="disabled || modelValue === 'simple'" @click="$emit('update:modelValue', 'simple')" data-testid="mode-simple">
      {{ simpleLabel }}
    </button>
  <button type="button" :class="['nav-button', { active: modelValue === 'advanced' }]" :disabled="disabled || modelValue === 'advanced'" @click="$emit('update:modelValue', 'advanced')" data-testid="mode-advanced">
      {{ advancedLabel }}
    </button>
  </div>
</template>

<script setup lang="ts">
interface Props {
  modelValue: 'simple' | 'advanced';
  disabled?: boolean;
  simpleLabel?: string;
  advancedLabel?: string;
}

withDefaults(defineProps<Props>(), {
  disabled: false,
  simpleLabel: 'Text',
  advancedLabel: 'Advanced',
});

defineEmits<{
  (e: 'update:modelValue', value: 'simple' | 'advanced'): void;
}>();
</script>

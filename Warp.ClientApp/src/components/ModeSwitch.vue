<template>
  <div class="flex items-center gap-2">
  <button type="button" :class="['nav-button', { active: modelValue === 'Simple' }]" :disabled="disabled || modelValue === 'Simple'" @click="$emit('update:modelValue', 'Simple')" data-testid="mode-simple">
      {{ simpleLabel }}
    </button>
  <button type="button" :class="['nav-button', { active: modelValue === 'Advanced' }]" :disabled="disabled || modelValue === 'Advanced'" @click="$emit('update:modelValue', 'Advanced')" data-testid="mode-advanced">
      {{ advancedLabel }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { EditMode } from '../types/edit-mode';

interface Props {
  modelValue: EditMode;
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
  (e: 'update:modelValue', value: EditMode): void;
}>();
</script>

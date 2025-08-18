<template>
  
  <div class="flex flex-col">
    <label v-if="label" class="form-label floating-label order-1">{{ label }}</label>
    <textarea
      class="form-textarea order-2 bg-transparent"
      placeholder=" " style="z-index: 10; height:40px; overflow-y:hidden;"
      :maxlength="maxLength ?? undefined"
      :disabled="disabled"
      :aria-label="ariaLabel || 'Text'"
      :value="modelValue"
      @input="$emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />
    <label class="form-label floating-label order-1">{{ placeholder }}</label>
    <p v-if="helper" class="text-xs text-gray-400 mt-2">{{ helper }}</p>
  </div>
</template>

<script setup lang="ts">
interface Props {
  modelValue: string;
  placeholder?: string;
  helper?: string;
  maxLength?: number;
  disabled?: boolean;
  label?: string;
  ariaLabel?: string;
}

withDefaults(defineProps<Props>(), {
  modelValue: '',
  placeholder: '',
  helper: '',
  label: '',
  disabled: false,
});

defineEmits<{
  (e: 'update:modelValue', value: string): void;
}>();
</script>

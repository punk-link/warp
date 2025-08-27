<template>
    <div class="flex items-center gap-2">
        <button type="button" :class="['nav-button', { active: modelValue === EditMode.Simple }]"
            :disabled="disabled || modelValue === EditMode.Simple" @click="$emit('update:modelValue', EditMode.Simple)"
            data-testid="mode-simple">
            {{ simpleLabel }}
        </button>
        <button type="button" :class="['nav-button', { active: modelValue === EditMode.Advanced }]"
            :disabled="disabled || modelValue === EditMode.Advanced"
            @click="$emit('update:modelValue', EditMode.Advanced)" data-testid="mode-advanced">
            {{ advancedLabel }}
        </button>
    </div>
</template>

<script setup lang="ts">
import { EditMode } from '../types/edit-modes';


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

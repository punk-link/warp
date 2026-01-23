<template>
    <label class="form-label">
        {{ t('components.expirationSelect.label') || label }}
    </label>
    <div class="flex items-baseline">
        <i class="icofont-close text-primary text-base mr-2"></i>
        <select class="form-select" :disabled="disabled" :value="modelValue" @change="onChange"
            :aria-label="ariaLabel || label">
            <option v-for="opt in options" :key="opt" :value="opt">{{ t(`components.expirationSelect.options.${opt}`) || opt }}</option>
        </select>
    </div>
</template>

<script setup lang="ts">
import { ExpirationPeriod } from '../types/entries/enums/expiration-periods'
import { parseExpirationPeriod } from '../helpers/expiration-period-helper'
import { useI18n } from 'vue-i18n'


interface Props {
    modelValue: ExpirationPeriod;
    options: ExpirationPeriod[];
    label?: string;
    disabled?: boolean;
    ariaLabel?: string;
}


const { t } = useI18n()


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
    emit('update:modelValue', parseExpirationPeriod(val))
}
</script>

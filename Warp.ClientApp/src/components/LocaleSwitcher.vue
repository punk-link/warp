<template>
    <div>
        <label class="form-label">{{ t('language') }}</label>
        <div class="flex items-baseline">
            <i class="icofont-loop text-primary text-base mr-2" aria-hidden="true"></i>
            <select class="form-select" v-model="selected" @change="onChange" :aria-label="t('language')">
                <option v-for="loc in supportedLocales" :key="loc" :value="loc">{{ nativeName(loc) }}</option>
            </select>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watchEffect } from 'vue'
import { useI18n } from 'vue-i18n'
import { supportedLocales, setLocale, getCurrentLocale } from '../i18n'


const { t } = useI18n()
const selected = ref<string>(getCurrentLocale())


function nativeName(locale: string): string {
    try {
        return new Intl.DisplayNames([locale], { type: 'language' }).of(locale) ?? locale
    } catch {
        return locale
    }
}


async function onChange() {
    await setLocale(selected.value as typeof supportedLocales[number])
}


watchEffect(() => {
    selected.value = getCurrentLocale()
})
</script>

<template>
    <select v-model="selected" @change="onChange"
        :aria-label="t('language')"
        class="appearance-none bg-transparent border-none text-gray-400 cursor-pointer hover:underline focus:outline-none px-1 py-0.5">
        <option v-for="loc in supportedLocales" :key="loc" :value="loc">{{ localeDisplayName(loc) }}</option>
    </select>
</template>

<script setup lang="ts">
import { ref, watchEffect } from 'vue'
import { useI18n } from 'vue-i18n'
import { supportedLocales, setLocale, getCurrentLocale } from '../i18n'


const { t } = useI18n()
const selected = ref<string>(getCurrentLocale())


async function onChange() {
    await setLocale(selected.value as typeof supportedLocales[number])
}


function localeDisplayName(loc: string): string {
    const name = new Intl.DisplayNames([loc], { type: 'language' }).of(loc) ?? loc
    return name.charAt(0).toUpperCase() + name.slice(1)
}


watchEffect(() => {
    selected.value = getCurrentLocale()
})
</script>

<template>
    <label class="inline-flex items-center gap-2 text-sm" aria-label="Locale selector">
        <span class="font-medium">{{ t('app.actions.close') ? 'Language' : 'Language' }}</span>
        <select v-model="selected" @change="onChange"
            class="border rounded px-2 py-1 bg-white dark:bg-neutral-800 dark:border-neutral-600 focus:outline-none focus:ring">
            <option v-for="loc in supportedLocales" :key="loc" :value="loc">{{ loc.toUpperCase() }}</option>
        </select>
    </label>
</template>

<script setup lang="ts">
import { ref, watchEffect } from 'vue'
import { useI18n } from 'vue-i18n'
import { supportedLocales, setLocale, currentLocale } from '../i18n'


const { t } = useI18n()
const selected = ref<string>(currentLocale())


async function onChange() {
    await setLocale(selected.value as typeof supportedLocales[number])
}


watchEffect(() => {
    selected.value = currentLocale()
})
</script>

<style scoped>
select {
    min-width: 4.5rem;
}
</style>

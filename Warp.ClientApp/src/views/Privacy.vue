<template>
    <div class="container mx-auto px-4 py-6">
        <article v-if="html" class="privacy-content" v-html="html"></article>
        <p v-else class="text-gray-500">{{ t('privacy.loading') }}</p>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const html = ref<string>('');

onMounted(async () => {
    try {
        const res = await fetch('/privacy.html', { cache: 'no-cache' });
        if (res.ok) {
            const raw = await res.text();
            const doc = new DOMParser().parseFromString(raw, 'text/html');
            doc.querySelectorAll('style, script').forEach(el => el.remove());
            html.value = doc.body.innerHTML;
        } else {
            console.error('Failed to load privacy policy:', res.statusText);
            html.value = `<h1>${t('privacy.notAvailableTitle')}</h1><p>${t('privacy.notAvailableContent')}</p>`;
        }
    } catch {
        html.value = `<h1>${t('privacy.notAvailableTitle')}</h1><p>${t('privacy.notAvailableContent')}</p>`;
    }
});
</script>

<style scoped>
.privacy-content :deep(h1) {
    font-size: 2rem;
    margin-bottom: 1rem;
}

.privacy-content :deep(p) {
    margin: 0.5rem 0;
}

.privacy-content :deep(a) {
    color: #2563eb;
    text-decoration: underline;
}
</style>

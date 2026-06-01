<template>
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
            </div>
            <div class="w-full sm:w-1/2 flex justify-end md:justify-center service-message-container">
                <span v-if="copied" id="link-copied-message" class="text-secondary font-semibold text-base">
                    {{ t('entry.linkCopied') }}
                </span>
                <span v-else-if="reported" class="text-secondary font-semibold text-base">
                    {{ t('entry.reported') }}
                </span>
            </div>
        </div>

        <section class="px-3 my-5">
            <div class="flex flex-col items-center justify-around min-h-[75vh]">
                <div class="w-full md:w-1/2 flex justify-between items-baseline mb-5">
                    <div class="flex items-center">
                        <i class="icofont-eye text-gray-400 mr-2"></i>
                        <span class="text-gray-600 font-semibold">{{ animatedViewCount }}</span>
                    </div>
                    <CountdownTimer v-if="!loading && !error" :target="countdownTarget" />
                </div>

                <article class="w-full md:w-1/2 p-3 rounded-sm mb-10">
                    <div v-if="images.length" ref="galleryElement" class="gallery mb-5 grid grid-cols-3 gap-2">
                        <template v-for="(img, i) in images" :key="img.id || i">
                            <Transition name="reveal-tile" mode="out-in">
                                <a v-if="!img.isSensitive || revealedImages.has(img.id)" :key="`revealed-${img.id}`" :href="img.url" data-fancybox="entry" class="block focus:outline-none focus:ring-2 focus:ring-primary rounded-sm">
                                    <GalleryItem :id="img.id" :src="img.url" :editable="false" />
                                </a>
                                <div v-else :key="`blurred-${img.id}`" class="block">
                                    <GalleryItem :id="img.id" :src="img.url" :editable="false" :is-sensitive="true" @reveal="onRevealImage(img.id)" />
                                </div>
                            </Transition>
                        </template>
                    </div>
                    <div v-if="entry?.editMode === EditMode.Simple" class="relative">
                        <div :class="['text-content font-sans-serif text-base whitespace-pre-wrap break-words transition-[filter,opacity,transform] duration-500 ease-out', { visible: showTextContent, 'select-none blur-sm pointer-events-none': isTextBlurred }]">
                            {{ entry?.textContent }}
                        </div>
                        <Transition name="reveal-overlay" appear>
                            <div v-if="isTextBlurred" class="absolute inset-0 flex items-center justify-center bg-white/75">
                                <div class="reveal-overlay-card flex flex-col items-center gap-3 text-center px-4">
                                    <i class="icofont-eye-blocked text-3xl text-gray-500" aria-hidden="true"></i>
                                    <span class="text-sm text-gray-600 font-semibold select-none">{{ t('entry.sensitiveContent.label') }}</span>
                                    <Button variant="outline-secondary" :label="t('entry.sensitiveContent.reveal')" icon-class="icofont-eye text-xl" @click="onRevealText" />
                                </div>
                            </div>
                        </Transition>
                    </div>
                    <div v-else-if="entry?.editMode === EditMode.Advanced" class="relative">
                        <div :class="['text-content rich-text-content font-sans-serif text-base transition-[filter,opacity,transform] duration-500 ease-out', { visible: showTextContent, 'select-none blur-sm pointer-events-none': isTextBlurred }]" v-html="sanitizedHtml"></div>
                        <Transition name="reveal-overlay" appear>
                            <div v-if="isTextBlurred" class="absolute inset-0 flex items-center justify-center bg-white/75">
                                <div class="reveal-overlay-card flex flex-col items-center gap-3 text-center px-4">
                                    <i class="icofont-eye-blocked text-3xl text-gray-500" aria-hidden="true"></i>
                                    <span class="text-sm text-gray-600 font-semibold select-none">{{ t('entry.sensitiveContent.label') }}</span>
                                    <Button variant="outline-secondary" :label="t('entry.sensitiveContent.reveal')" icon-class="icofont-eye text-xl" @click="onRevealText" />
                                </div>
                            </div>
                        </Transition>
                    </div>
                    <div v-if="loading" class="text-center text-gray-400 py-10">
                        {{ t('entry.loading') }}
                    </div>
                </article>

                <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <div class="bg-white rounded-sm">
                        <Button variant="outline-secondary" :disabled="reporting || loading || error" :pending="reporting" @click="showReportModal = true" :title="t('entry.actions.report')" icon-class="icofont-exclamation-tringle text-xl" />
                    </div>
                    <div class="bg-white rounded-sm">
                        <Button variant="gray" @click="onClose" :label="t('entry.actions.close')" icon-class="icofont-close text-xl" />
                    </div>
                    <div class="bg-white rounded-sm">
                        <Button variant="outline-gray" :disabled="loading || error" @click="onCopyLink" :title="t('entry.actions.copyLink')" icon-class="icofont-link text-xl" />
                    </div>
                </div>
            </div>
        </section>
    </div>

    <div v-if="showReportModal" class="fixed inset-0 flex items-center justify-center">
        <div class="absolute inset-0 bg-black opacity-20" @click="showReportModal = false"></div>
        <div class="bg-white rounded-lg w-11/12 md:w-1/3 shadow-lg z-10">
            <div class="p-6">
                <div class="flex flex-col items-center mb-10">
                    <i class="icofont-exclamation-tringle text-7xl text-secondary"></i>
                </div>
                <div class="text-center px-4">
                    <p class="font-sans-serif text-lg text-gray-600 mb-4">
                        {{ t('entry.reportModal.description') }}
                    </p>
                </div>
            </div>
            <div class="flex justify-end gap-3 p-4 border-t border-gray-200">
                <Button variant="outline-gray" type="button" @click="showReportModal = false" :label="t('app.actions.cancel')" />
                <Button variant="primary" type="button" :disabled="reporting" :pending="reporting" @click="onReport" :label="reporting ? t('entry.reportModal.reporting') : t('entry.reportModal.confirm')" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onBeforeUnmount, nextTick, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { entryApi } from '../api/entry-api'
import { routeApiError } from '../api/error-routing'
import { sanitize } from '../helpers/sanitize-html'
import { EditMode } from '../types/entries/enums/edit-modes'
import type { Entry } from '../types/entries/entry'
import CountdownTimer from '../components/CountdownTimer.vue'
import GalleryItem from '../components/GalleryItem.vue'
import Logo from '../components/Logo.vue'
import Button from '../components/Button.vue'
import { ViewNames } from '../router/enums/view-names'


interface EntryImage {
    entryId: string
    id: string
    url: string
    isSensitive: boolean
}

const route = useRoute()
const router = useRouter()
const { t } = useI18n()

const props = defineProps<{ id?: string }>()
const entry = ref<Entry | null>(null)
const loading = ref(false)
const error = ref(false)
const copied = ref(false)
const reported = ref(false)
const reporting = ref(false)
const showReportModal = ref(false)
const images = ref<EntryImage[]>([])
const revealedImages = ref(new Set<string>())
const showTextContent = ref(false)
const textRevealed = ref(false)
const animatedViewCount = ref(0)
const countdownTarget = ref<Date | string | null>(null)
let expirationTimer: number | null = null
let fancyboxBound = false


const sanitizedHtml = computed(() => {
    if (!entry.value?.textContent)
        return ''
    
    return sanitize(entry.value.textContent)
})


const isTextBlurred = computed(() => {
    if (!entry.value?.isTextBlurred)
        return false

    return !textRevealed.value
})


function animateCount(target: number, durationMs = 1200) {
    const start = performance.now()
    const from = 0

    function step(ts: number) {
        const progressPercentage = Math.min(1, (ts - start) / durationMs)
        animatedViewCount.value = Math.round(from + (target - from) * progressPercentage)
        if (progressPercentage < 1)
            requestAnimationFrame(step)
    }

    requestAnimationFrame(step)
}


function applyEntryData(fetchedEntry: Entry) {
    entry.value = fetchedEntry
    images.value = processImages(fetchedEntry)
    countdownTarget.value = fetchedEntry.expiresAt
    revealedImages.value = new Set()
    showTextContent.value = false
    textRevealed.value = false
    
    if (fetchedEntry.textContent)
        setTimeout(() => showTextContent.value = true, 100)
    
    animateCount(fetchedEntry.viewCount || 0)
}


function bindFancybox() {
    if (!(window as any).Fancybox)
        return

    const Fb = (window as any).Fancybox

    if (fancyboxBound) {
        try { Fb.unbind('[data-fancybox="entry"]') } catch { /* noop */ }
        fancyboxBound = false
    }

    Fb.bind('[data-fancybox="entry"]', {
        placeFocusBack: true,
        groupAll: true,
        Thumbs: false
    })

    fancyboxBound = true
}


function getCurrentId(): string | null {
    const currentId = props.id || (route.params.id as string | undefined)
    
    if (!currentId) {
        error.value = true
        router.replace({ name: ViewNames.Error })
        return null
    }
    
    return currentId
}

async function load() {
    const currentId = getCurrentId()
    if (!currentId)
        return

    try {
        loading.value = true
        const fetchedEntry = await entryApi.getEntry(currentId)
        
        applyEntryData(fetchedEntry)
        scheduleExpirationRedirect()
        
        await nextTick()
        bindFancybox()
    } catch (e) {
        console.error('failed to load entry', e)
        error.value = true
        routeApiError(e)
    } finally {
        loading.value = false
    }
}


onBeforeUnmount(() => {
    if (expirationTimer) {
        clearTimeout(expirationTimer)
        expirationTimer = null
    }

    if (fancyboxBound && (window as any).Fancybox) {
        try {
            (window as any).Fancybox.unbind('[data-fancybox="entry"]')
        } catch {
            console.warn('failed to unbind Fancybox')
        }
    }
})


function onClose() {
    router.replace({ name: ViewNames.Home })
}


async function onCopyLink() {
    const currentId = props.id
    if (!currentId)
        return

    try {
        const link = `${window.location.origin}/entry/${encodeURIComponent(currentId)}`
        await navigator.clipboard.writeText(link)

        copied.value = true

        setTimeout(() => copied.value = false, 2500)
    } catch (e) {
        console.error('copy failed', e)
    }
}


async function onReport() {
    const currentId = props.id
    if (!currentId || reporting.value)
        return

    try {
        reporting.value = true
        await entryApi.reportEntry(currentId)

        reported.value = true
        showReportModal.value = false

        setTimeout(() => router.replace({ name: ViewNames.Home }), 1200)
    } catch (e) {
        console.error('report failed', e)
    } finally {
        reporting.value = false
    }
}


function processImages(fetchedEntry: Entry): EntryImage[] {
    if (!Array.isArray(fetchedEntry.images))
        return []

    return fetchedEntry.images.map(imageInfo => ({
        entryId: imageInfo.entryId,
        id: imageInfo.id,
        url: imageInfo.url,
        isSensitive: imageInfo.isBlurred
    }))
}


function onRevealImage(imageId: string) {
    const updated = new Set(revealedImages.value)
    updated.add(imageId)
    revealedImages.value = updated

    nextTick().then(bindFancybox)
}


function onRevealText() {
    showTextContent.value = true
    textRevealed.value = true
}


function scheduleExpirationRedirect() {
    if (!countdownTarget.value)
        return

    if (expirationTimer) {
        clearTimeout(expirationTimer)
        expirationTimer = null
    }

    const targetTs = new Date(countdownTarget.value).getTime()
    const now = Date.now()
    const delay = targetTs - now
    if (delay <= 0) {
        router.replace({ name: ViewNames.Error })
        return
    }

    expirationTimer = window.setTimeout(() => {
        router.replace({ name: ViewNames.Home })
    }, delay)
}


onMounted(load)


watch(() => props.id, () => { load() })

// Rebind Fancybox if images array changes independently (defensive)
watch(images, () => {
    nextTick().then(bindFancybox)
})
</script>

<style scoped>
.reveal-overlay-enter-active,
.reveal-overlay-leave-active {
    transition: opacity 220ms ease;
}


.reveal-overlay-enter-from,
.reveal-overlay-leave-to {
    opacity: 0;
}


.reveal-overlay-card {
    transition: transform 280ms ease, opacity 220ms ease;
}


.reveal-overlay-enter-from .reveal-overlay-card,
.reveal-overlay-leave-to .reveal-overlay-card {
    opacity: 0;
    transform: translateY(10px) scale(0.96);
}


.reveal-tile-enter-active,
.reveal-tile-leave-active {
    transition: opacity 280ms ease, transform 320ms ease, filter 320ms ease;
}


.reveal-tile-enter-from,
.reveal-tile-leave-to {
    opacity: 0;
    filter: blur(8px);
    transform: scale(0.96);
}
</style>

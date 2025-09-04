<template>
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
            </div>
            <div class="w-full sm:w-1/2 flex justify-end md:justify-center service-message-container">
                <span v-if="copied" id="link-copied-message" class="text-secondary font-semibold text-base">
                    link copied
                </span>
                <span v-else-if="reported" class="text-secondary font-semibold text-base">
                    reported
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
                    <div v-if="images.length" class="gallery mb-5 grid grid-cols-3 gap-2">
                        <GalleryItem v-for="(img, i) in images" :key="img.id || i" :id="img.id" :src="img.url" :editable="false" />
                    </div>
                    <div ref="textContentEl" class="text-content font-sans-serif text-base whitespace-pre-wrap break-words">
                        {{ entry?.textContent }}
                    </div>
                    <div v-if="loading" class="text-center text-gray-400 py-10">
                        loading...
                    </div>
                </article>

                <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <div class="bg-white rounded-sm">
                        <Button variant="outline-secondary" :disabled="reporting || loading || error" @click="showReportModal = true" icon-class="icofont-exclamation-tringle text-xl" />
                    </div>
                    <div class="bg-white rounded-sm">
                        <Button variant="gray" label="Close" @click="onClose" icon-class="icofont-close text-xl" />
                    </div>
                    <div class="bg-white rounded-sm">
                        <Button variant="outline-gray" :disabled="loading || error" @click="onCopyLink" icon-class="icofont-link text-xl" />
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
                        You are about to report this content. Use the feature in case of inappropriate content only.
                        This action restricts access to the content for all viewers. Are you sure?
                    </p>
                </div>
            </div>
            <div class="flex justify-end gap-3 p-4 border-t border-gray-200">
                <Button variant="outline-gray" type="button" @click="showReportModal = false" label="Cancel" />
                <Button variant="primary" type="button" :disabled="reporting" @click="onReport" :label="reporting ? 'Reporting...' : 'Report'" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { entryApi } from '../api/entryApi'
import type { Entry } from '../types/entry'
import CountdownTimer from '../components/CountdownTimer.vue'
import GalleryItem from '../components/GalleryItem.vue'
import Logo from '../components/Logo.vue'
import Button from '../components/Button.vue'

const route = useRoute()
const router = useRouter()

const props = defineProps<{ id?: string }>()

const entry = ref<Entry | null>(null)
const loading = ref(false)
const error = ref(false)
const copied = ref(false)
const reported = ref(false)
const reporting = ref(false)
const showReportModal = ref(false)
interface EntryImage { entryId: string; id: string; url: string }
const images = ref<EntryImage[]>([])
const textContentEl = ref<HTMLElement | null>(null)
const animatedViewCount = ref(0)
const countdownTarget = ref<Date | string | null>(null)
let expirationTimer: number | null = null


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

async function load() {
    const currentId = props.id || (route.params.id as string | undefined)

    if (!currentId) { 
        error.value = true; 
        router.replace({ name: 'Error' })
        return 
    }

    try {
        loading.value = true

        const fetchedEntry = await entryApi.getEntry(currentId)
        entry.value = fetchedEntry as Entry
        if (Array.isArray((fetchedEntry as any).images)) {
            images.value = (fetchedEntry as any).images
                .map((imageInfo: any) => {
                    return {
                        entryId: typeof imageInfo.entryId === 'string' ? imageInfo.entryId : (fetchedEntry.id ?? ''),
                        id: imageInfo.id,
                        url: imageInfo.url
                    } as EntryImage
                })
                .filter((x: EntryImage | null): x is EntryImage => !!x)
        } else {
            images.value = []
        }

        countdownTarget.value = fetchedEntry.expiresAt

        if (fetchedEntry.textContent) 
            setTimeout(() => { textContentEl.value?.classList.add('visible') }, 100)

        animateCount(fetchedEntry.viewCount || 0)
    
        scheduleExpirationRedirect()
    } catch (e) {
        console.error('failed to load entry', e)
        error.value = true
        router.replace({ name: 'Error' })
    } finally {
        loading.value = false
    }
}


async function onCopyLink() {
    const currentId = props.id

    if (!currentId) 
        return

    try {
        const link = `${window.location.origin}/app/entry/${encodeURIComponent(currentId)}`
        await navigator.clipboard.writeText(link)
        
        copied.value = true

        setTimeout(() => copied.value = false, 2500)
    } catch (e) { 
        console.error('copy failed', e) 
    }
}


function onClose() {
    router.replace({ name: 'Home' })
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

        setTimeout(() => router.replace({ name: 'Home' }), 1200)
    } catch (e) {
        console.error('report failed', e)
    } finally { 
        reporting.value = false
    }
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
        router.replace({ name: 'Error' })
        return
    }
    
    expirationTimer = window.setTimeout(() => {
        router.replace({ name: 'Home' })
    }, delay)
}


onBeforeUnmount(() => {
    if (expirationTimer) {
        clearTimeout(expirationTimer)
        expirationTimer = null
    }
})


onMounted(load)


watch(() => props.id, () => { load() })
</script>

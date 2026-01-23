<template>
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
            </div>
        </div>

        <section class="px-3 my-5">
            <div class="flex flex-col items-center justify-around min-h-[75vh]">
                <div class="w-full md:w-1/2 mb-5">
                    <div class="flex flex-col">
                        <div class="text-white text-9xl font-sans-serif">
                            {{ status }}
                        </div>

                        <div class="mb-8">
                            <p class="mb-10 text-white text-xl">
                                {{ message }}
                            </p>

                            <div v-if="isServerError && requestId" class="text-base text-gray-700 text-sm relative">
                                <span class="font-semibold">{{ t('error.requestIdLabel') }}</span>
                                <code class="bg-gray-100 p-1 rounded border border-transparent hover:bg-gray-50 hover:border-secondary cursor-pointer transition-all duration-200"
                                    :title="copied ? 'Copied!' : 'Click to copy'" @click="copy">
                                    <span class="text-gray-400 select-none">REQ-</span>
                                    <span>{{ shortId }}</span>
                                    <span class="text-gray-500" v-if="requestId.length > 8">...</span>
                                    <i class="icofont-copy ml-1 text-gray-400"></i>
                                </code>
                                <span class="absolute -top-8 left-1/2 -translate-x-1/2 text-xs font-semibold text-white bg-secondary px-2 py-1 rounded shadow-md transition-opacity duration-200"
                                    :class="copied ? 'opacity-100' : 'opacity-0 pointer-events-none'">
                                    {{ t('app.actions.copy') }}
                                </span>
                                <p class="text-xs text-gray-500 mt-1">
                                    {{ t('error.requestIdLabel') }}
                                </p>
                            </div>

                            <div v-if="isServerError && errorItems.length" class="mt-4">
                                <h3 class="text-xl text-gray-300 font-semibold mb-2">{{ t('error.errorsTitle') }}</h3>
                                <div v-for="(err, idx) in errorItems" :key="idx" class="mb-1 text-sm text-gray-400">
                                    <span class="font-semibold">{{ err.code }}.</span>
                                    <span class="ml-1">{{ err.message }}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="flex justify-center items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <div>
                        <Button variant="primary" icon-class="icofont-simple-left text-white/50"
                            :label="t('error.goHome')" @click="goHome" />
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import Button from '../components/Button.vue'
import Logo from '../components/Logo.vue'
import { ViewNames } from '../router/enums/view-names'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()


const hasErrorParams = computed(() => ['status', 'title', 'detail', 'rid'].some(k => route.query[k] != null))


const status = computed<number>(() => {
    if (!hasErrorParams.value)
        return 404

    const rawStatus = route.query.status as string | undefined
    const parsed = rawStatus
        ? parseInt(rawStatus, 10)
        : NaN

    return Number.isFinite(parsed)
        ? parsed
        : 500
})


const message = computed<string>(() => {
    if (!hasErrorParams.value)
        return t('error.defaultNotFound')

    const title = route.query.title as string | undefined
    const detailRaw = route.query.detail as string | undefined
    const parsedDetail = parsePossibleJson(detailRaw)

    return title || parsedDetail || t('error.defaultUnexpected')
})


const requestId = computed(() => (hasErrorParams.value ? (route.query.rid as string | undefined) : undefined) || '')
const shortId = computed(() => requestId.value.substring(0, 8))
const copied = ref(false)


const isServerError = computed(() => status.value >= 500)


interface Item { code: string; message: string }
const errorItems = computed<Item[]>(() => {
    if (!isServerError.value)
        return []

    const rawErrors = route.query.errs as string | undefined
    if (!rawErrors)
        return []

    return rawErrors.split('|')
        .map(segment => {
            const [c, ...rest] = segment.split(':')
            return { code: (c || '').trim(), message: rest.join(':').trim() }
        })
        .filter(e => e.code || e.message)
})


async function copy() {
    if (!requestId.value)
        return

    try {
        await navigator.clipboard.writeText(requestId.value)
        copied.value = true
        setTimeout(() => copied.value = false, 1800)
    } catch (e) {
        console.error('copy request id failed', e)
    }
}

function goHome() {
    router.push({ name: ViewNames.Home })
}


function parsePossibleJson(input?: string | null): string | undefined {
    if (!input)
        return undefined

    let potentialValue = input
    try {
        potentialValue = decodeURIComponent(input)
    } catch {
        // ignore decode errors, keep original
    }

    // Some encoders use '+' for spaces in query-string context; normalize those
    potentialValue = potentialValue.replace(/\+/g, ' ')

    try {
        const parsed = JSON.parse(potentialValue)
        if (parsed && typeof parsed === 'object') {
            if (typeof parsed.detail === 'string' && parsed.detail)
                return parsed.detail

            if (typeof parsed.message === 'string' && parsed.message)
                return parsed.message
            
            if (typeof parsed.title === 'string' && parsed.title)
                return parsed.title
        }
    } catch (e) {
        // not JSON, fall through to return normalized string
    }

    return potentialValue
}
</script>
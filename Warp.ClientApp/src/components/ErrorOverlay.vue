<template>
    <Teleport to="body">
        <div v-if="current"
            class="fixed inset-0 z-50 flex flex-col items-center justify-center md:hidden"
            role="alertdialog"
            aria-modal="true"
            :aria-labelledby="titleId"
            :aria-describedby="subtitleId">
            <div class="absolute inset-0 bg-black/75" aria-hidden="true"></div>

            <div class="relative flex flex-col items-center px-8 py-10 text-center animate-overlay-slide-up">
                <p :id="titleId" class="text-2xl font-semibold text-white lowercase">
                    {{ t('components.errorOverlay.title') }}
                </p>

                <p v-if="current.message" class="mt-3 text-sm text-white/80">
                    {{ current.message }}
                </p>

                <p :id="subtitleId" class="mt-4 text-base text-white/90 lowercase">
                    {{ t('components.errorOverlay.subtitle') }}
                </p>

                <div v-if="current.traceId" class="mt-5 flex flex-col items-center gap-1">
                    <span class="text-xs text-white/60 uppercase tracking-wide">
                        {{ t('components.errorOverlay.traceIdLabel') }}
                    </span>
                    <code
                        class="relative text-xs text-white/90 bg-white/10 px-3 py-1.5 rounded cursor-pointer select-all hover:bg-white/20 transition-colors duration-150"
                        :title="copied ? t('components.errorOverlay.copied') : current.traceId"
                        @click="copyDiagnostics">
                        {{ current.traceId }}
                    </code>
                    <span
                        class="text-xs font-semibold text-white transition-opacity duration-200"
                        :class="copied ? 'opacity-100' : 'opacity-0'">
                        {{ t('components.errorOverlay.copied') }}
                    </span>
                </div>

                <div class="mt-8 flex gap-4">
                    <Button
                        variant="gray"
                        :label="t('components.errorOverlay.back')"
                        @click="onBack" />

                    <Button
                        v-if="current.onRetry"
                        variant="primary"
                        :label="t('components.errorOverlay.tryAgain')"
                        :pending="retrying"
                        @click="onTryAgain" />
                </div>
            </div>
        </div>
    </Teleport>
</template>


<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useNotifications } from '../composables/use-notifications'
import Button from './Button.vue'


const { t } = useI18n()
const router = useRouter()
const { items, remove } = useNotifications()

const titleId = 'error-overlay-title'
const subtitleId = 'error-overlay-subtitle'

const copied = ref(false)
const retrying = ref(false)

const current = computed(() => {
    const arr = items.value ?? []
    for (let i = arr.length - 1; i >= 0; i--) {
        if (arr[i].showAsOverlay)
            return arr[i]
    }
    
    return null
})


function copyDiagnostics(): void {
    const text = current.value?.diagnostics ?? current.value?.traceId
    if (!text)
        return

    void navigator.clipboard?.writeText(text).then(() => {
        copied.value = true
        setTimeout(() => copied.value = false, 1800)
    })
}


function onBack(): void {
    if (current.value)
        remove(current.value.id)

    router.back()
}


async function onTryAgain(): Promise<void> {
    const notification = current.value
    if (!notification?.onRetry)
        return

    retrying.value = true
    try {
        await notification.onRetry()
        remove(notification.id)
    } finally {
        retrying.value = false
    }
}
</script>

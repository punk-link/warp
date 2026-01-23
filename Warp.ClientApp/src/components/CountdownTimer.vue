<template>
    <div v-if="display" class="text-right">
        <div ref="countdownEl" class="countdown text-gray-700 text-3xl font-sans-serif font-semibold tabular-nums" v-html="markup || '&nbsp;'"></div>
        <div class="text-gray-400 text-xs">{{ displayLabel }}</div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { useEntryCountdown } from '../composables/use-entry-countdown'
import { useI18n } from 'vue-i18n'


interface Props {
    target?: Date | string | null;
    label?: string | undefined;
    startDelayMs?: number;
}


const { t } = useI18n()


const props = withDefaults(defineProps<Props>(), {
    target: null,
    label: undefined,
    startDelayMs: 100
})


const displayLabel = computed(() => props.label ?? t('components.countdown.label') ?? 'the entry expires in')
const { markup, start } = useEntryCountdown()
const countdownEl = ref<HTMLElement | null>(null)
const started = ref(false)
const display = ref(true)


function tryStart() {
    if (started.value || !props.target)
        return

    started.value = true
    start(props.target)

    setTimeout(() => countdownEl.value?.classList.add('visible'), props.startDelayMs)
}


onMounted(() => {
    tryStart()
})


watch(() => props.target, () => {
    if (props.target) {
        started.value = false
        tryStart()
    }
})
</script>

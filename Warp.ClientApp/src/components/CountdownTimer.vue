<template>
    <div v-if="display" class="text-right">
        <div ref="countdownEl" class="countdown text-gray-700 text-3xl font-sans-serif font-semibold tabular-nums"
            v-html="markup || '&nbsp;'"></div>
        <div class="text-gray-400 text-xs">{{ label }}</div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useEntryCountdown } from '../composables/useEntryCountdown'


interface Props {
    target?: Date | string | null;
    label?: string;
    startDelayMs?: number;
}


const props = withDefaults(defineProps<Props>(), {
    target: null,
    label: 'the entry expires in',
    startDelayMs: 100
})


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

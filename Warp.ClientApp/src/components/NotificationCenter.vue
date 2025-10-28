<template>
    <Teleport to="body">
        <div class="fixed top-4 right-4 z-50 flex flex-col gap-3 items-end pointer-events-none">
            <div v-for="n in items" :key="n.id"
                class="w-[min(92vw,28rem)] pointer-events-auto rounded-md shadow-lg ring-1 ring-black/10 overflow-hidden"
                :class="containerClass(n.level)" :role="ariaRole(n.level)" :aria-live="ariaLive(n.level)"
                aria-atomic="true">
                <div class="p-3 pl-3 pr-2 sm:pl-4 sm:pr-3 flex items-start gap-3">
                    <div class="flex-1 min-w-0">
                        <p class="text-sm font-medium leading-5 break-words">
                            {{ n.message }}
                            <span v-if="n.occurrences > 1"
                                class="ml-2 inline-flex items-center rounded-full bg-black/20 px-2 py-0.5 text-[11px] font-semibold">
                                ×{{ n.occurrences }}
                            </span>
                        </p>
                        <p v-if="n.details" class="mt-1 text-xs opacity-90 break-words">
                            {{ n.details }}
                        </p>
                        <div v-if="n.actions?.length" class="mt-2 flex flex-wrap gap-2">
                            <button v-for="(a, i) in n.actions" :key="i" type="button"
                                class="inline-flex items-center rounded border border-white/30 bg-white/10 px-2 py-1 text-xs font-medium hover:bg-white/20 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-white/60"
                                :title="a.title || a.label" @click="a.onClick()">
                                {{ a.label }}
                            </button>
                        </div>
                    </div>

                    <button type="button"
                        class="shrink-0 inline-flex items-center justify-center rounded-md p-1 opacity-80 hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-white/60"
                        :aria-label="'Close notification'" title="Close" @click="remove(n.id)">
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="h-4 w-4">
                            <path fill-rule="evenodd"
                                d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                                clip-rule="evenodd" />
                        </svg>
                    </button>
                </div>
            </div>
        </div>
    </Teleport>
</template>


<script setup lang="ts">
import { computed } from 'vue'
import { useNotifications } from '../composables/useNotifications'
import { NotifyLevel } from '../types/notify-level'


const { items, remove } = useNotifications()


function containerClass(level: NotifyLevel): string {
    if (level === NotifyLevel.Error)
        return 'bg-red-600/95 text-white'

    if (level === NotifyLevel.Warn)
        return 'bg-indigo-600/90 text-white'

    return 'bg-sky-600/90 text-white'
}


function ariaRole(level: NotifyLevel): 'alert' | 'status' {
    return level === NotifyLevel.Error ? 'alert' : 'status'
}


function ariaLive(level: NotifyLevel): 'assertive' | 'polite' {
    return level === NotifyLevel.Error ? 'assertive' : 'polite'
}
</script>

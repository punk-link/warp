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
                            {{ n.title }}
                            <span v-if="n.occurrences > 1" class="ml-2 text-[11px] font-semibold">
                                ×{{ n.occurrences }}
                            </span>
                        </p>
                        <p v-if="n.message" class="mt-1 text-sm leading-5 opacity-90">
                            {{ n.message }}
                        </p>
                        <p v-if="n.details" class="mt-2 text-xs opacity-90 break-words whitespace-pre-line">
                            {{ n.details }}
                        </p>
                        <div v-if="n.actions?.length" class="mt-2 flex flex-wrap gap-2">
                            <Button v-for="(a, i) in n.actions" :key="i" variant="outline-gray" :label="a.label" :title="a.title || a.label"
                                class="!bg-white/10 !text-white hover:!bg-white/20 focus:!ring-2 focus:!ring-offset-2 focus:!ring-white/60 text-xs"
                                @click="a.onClick()"
                            />
                        </div>
                    </div>

                    <Button variant="outline-gray" :label="null" :aria-label="t('components.notifications.ariaClose')" 
                        :title="t('components.notifications.close')" icon-class="icofont-close text-white/90" @click="remove(n.id)"
                    />
                </div>
            </div>
        </div>
    </Teleport>
</template>


<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useNotifications } from '../composables/use-notifications'
import { NotificationLevel } from '../types/notifications/enums/notification-level'
import Button from './Button.vue'


const { items, remove } = useNotifications()
const { t } = useI18n()


function containerClass(level: NotificationLevel): string {
    if (level === NotificationLevel.Error)
        return 'bg-red-600/95 text-white'

    if (level === NotificationLevel.Warn)
        return 'bg-secondary/90 text-white'

    return 'bg-primary/90 text-white'
}


function ariaRole(level: NotificationLevel): 'alert' | 'status' {
    return level === NotificationLevel.Error ? 'alert' : 'status'
}


function ariaLive(level: NotificationLevel): 'assertive' | 'polite' {
    return level === NotificationLevel.Error ? 'assertive' : 'polite'
}
</script>

import { computed, type Ref } from 'vue'
import { useI18n } from 'vue-i18n'


/**
 * Content size limits in bytes (matching backend validation)
 */
export const maxPlainTextContentSize = window.appConfig?.maxPlainTextContentSize ?? 262144
export const maxHtmlContentSize = window.appConfig?.maxHtmlContentSize ?? 262144
export const maxContentDeltaSize = window.appConfig?.maxContentDeltaSize ?? 524288


/**
 * Calculates the UTF-8 byte size of a string
 */
export function getByteSize(content: string): number {
    return new Blob([content]).size
}


/**
 * Gets the circle indicator color and warning key based on content size
 */
export function getSizeIndicatorState(sizeBytes: number, maxSizeBytes: number): {
    circleColor: string
    warningKey: string | null
    showWarning: boolean
} {
    const percentage = (sizeBytes / maxSizeBytes) * 100

    if (percentage >= 100) {
        return {
            circleColor: 'bg-red-500',
            warningKey: 'exceeded',
            showWarning: true,
        }
    }

    if (percentage >= 90) {
        return {
            circleColor: 'bg-orange-500',
            warningKey: 'approaching',
            showWarning: true,
        }
    }

    if (percentage >= 75) {
        return {
            circleColor: 'bg-yellow-500',
            warningKey: 'large',
            showWarning: true,
        }
    }

    return {
        circleColor: 'bg-green-500',
        warningKey: null,
        showWarning: false,
    }
}


/**
 * Composable for tracking content size and providing visual feedback
 */
export function useContentSizeIndicator(content: Ref<string>, maxSize: number) {
    const { t } = useI18n()
    const sizeBytes = computed(() => getByteSize(content.value))
    const indicatorState = computed(() => getSizeIndicatorState(sizeBytes.value, maxSize))
    const isOverLimit = computed(() => sizeBytes.value > maxSize)

    const warningText = computed(() => {
        const key = indicatorState.value.warningKey
        if (!key)
            return null
        
        return t(`components.contentSizeIndicator.${key}`)
    })

    return {
        sizeBytes,
        circleColor: computed(() => indicatorState.value.circleColor),
        warningText,
        showWarning: computed(() => indicatorState.value.showWarning),
        isOverLimit,
    }
}

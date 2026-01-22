import { ref, onBeforeUnmount } from 'vue'


/** Composable for displaying a countdown to a entry's release time. */
export function useEntryCountdown() {
    return { markup, start, stop }
}

function buildCountdownMarkup(timeString: string, significantIndex: number) {
    const digitSpanClass = 'inline-block w-[1ch] text-center leading-none'
    const colonSpanClass = 'inline-block text-center leading-none'
    const chars = timeString.split('')
    return chars.map((ch, i) => {
        if (ch === ':') {
            const color = i < significantIndex ? 'text-gray-300' : 'text-secondary'
            return `<span class=\"${colonSpanClass} ${color}\">:</span>`
        }

        const color = i < significantIndex ? 'text-gray-300' : ''
        return `<span class=\"${digitSpanClass} ${color}\">${ch}</span>`
    }).join('')
}


function calculateSegments(remaining: number) {
    return {
        hours: Math.floor((remaining / MILLISECONDS.HOUR) % 24),
        minutes: Math.floor((remaining / MILLISECONDS.MINUTE) % 60),
        seconds: Math.floor((remaining / MILLISECONDS.SECOND) % 60)
    }
}


function findFirstSignificantDigitIndex(timeString: string) {
    const idx = timeString.split('')
        .findIndex(c => c !== '0' && c !== ':')
    return idx === -1 ? 0 : idx
}


function formatCountdown(hours: number, minutes: number, seconds: number) {
    if (!hasRemaining({ hours, minutes, seconds })) 
        return getEmptyCountdownMarkup()
    
    const timeString = [hours, minutes, seconds].map(formatTimeSection).join(':')
    const sig = findFirstSignificantDigitIndex(timeString)
    
    if (sig === 0) 
        return getEmptyCountdownMarkup()
    
    return buildCountdownMarkup(timeString, sig)
}


function formatTimeSection(n: number) { 
    return n.toString().padStart(2, '0') 
}


function getEmptyCountdownMarkup() {
    return buildCountdownMarkup('00:00:00', 0)
}


function hasRemaining(seg: { hours: number; minutes: number; seconds: number }) {
    return seg.hours > 0 || seg.minutes > 0 || seg.seconds > 0
}


function scheduleNextAlignedTick(tick: () => void) {
    const now = Date.now()
    const delay = MILLISECONDS.SECOND - (now % MILLISECONDS.SECOND)
    timeoutId = setTimeout(tick, delay)
}


function start(target: Date | string) {
    if (!target) 
        return
    
    stop()
    targetTime = (target instanceof Date) ? target.getTime() : new Date(target).getTime()
    requestAnimationFrame(() => {
        update()
    })
}

function stop() {
    if (timeoutId) {
        clearTimeout(timeoutId)
        timeoutId = null
    }
}


function update() {
    if (targetTime == null) 
        return
    
    const now = Date.now()
    const remaining = targetTime - now
    if (remaining <= 0) {
        markup.value = getEmptyCountdownMarkup()
        window.location.reload()
        return
    }

    const seg = calculateSegments(remaining)
    markup.value = formatCountdown(seg.hours, seg.minutes, seg.seconds)
    scheduleNextAlignedTick(() => requestAnimationFrame(update))
}


onBeforeUnmount(stop)


const markup = ref('')
let timeoutId: any = null
let targetTime: number | null = null

const MILLISECONDS = { 
    SECOND: 1_000, 
    MINUTE: 60_000, 
    HOUR: 3_600_000 
}

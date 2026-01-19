import { ref } from 'vue'
import { DraftEntry } from '../types/draft-entry'


const STORAGE_KEY = 'warp.draftEntry'
const draft = ref<DraftEntry | null>(null)


function loadFromStorage() {
    if (draft.value)
        return

    try {
        const json = sessionStorage.getItem(STORAGE_KEY)
        if (!json)
            return

        const parsed = JSON.parse(json)
        if (parsed && typeof parsed === 'object')
            draft.value = parsed
    } catch {
        console.log('Failed to load draft entry')
    }
}


function persist() {
    try {
        if (!draft.value) {
            sessionStorage.removeItem(STORAGE_KEY)
            return
        }

        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(draft.value))
    } catch {
        console.log('Failed to persist draft entry')
    }
}

function setDraft(d: DraftEntry) {
    draft.value = d;
    persist()
}


function clearDraft() {
    draft.value = null;
    persist()
}


loadFromStorage()


export function useDraftEntry() {
    return { draft, setDraft, clearDraft }
}

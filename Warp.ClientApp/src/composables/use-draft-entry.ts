import { ref } from 'vue'
import { DraftEntry } from '../types/entries/draft-entry'


const STORAGE_KEY = 'warp.draftEntry'
const draft = ref<DraftEntry | null>(null)


/**
 * Composable for restoring and persisting a draft entry.
 * Stores the complete entry state including textContent (HTML for Advanced mode, plain text for Simple mode)
 * and contentDelta (ProseMirror JSON for Advanced mode) in session storage.
 */
export function useDraftEntry() {
    return { draft, setDraft, clearDraft }
}


function clearDraft() {
    draft.value = null;
    persist()
}


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


loadFromStorage()

import { ref, watch } from 'vue'
import { EditMode, parseEditMode } from '../types/edit-modes'

const STORAGE_KEY = 'warp.editMode'

export function useEditMode() {
    const stored = typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null
    const initial = stored ? parseEditMode(stored) : EditMode.Simple
    const mode = ref<EditMode>(initial === EditMode.Unset ? EditMode.Simple : initial)

    watch(mode, (v) => {
        localStorage.setItem(STORAGE_KEY, v)
    })

    return { mode }
}

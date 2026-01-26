import { ref, watch } from 'vue'
import { EditMode } from '../types/entries/enums/edit-modes'
import { parseEditMode } from '../helpers/edit-mode-helper'


const STORAGE_KEY = 'warp.editMode'


/** Composable for restoring and persisting the user's selected edit mode. */
export function useEditMode() {
    const stored = typeof localStorage !== 'undefined' 
        ? localStorage.getItem(STORAGE_KEY) 
        : null

    const initial = stored 
        ? parseEditMode(stored) 
        : EditMode.Simple
        
    const mode = ref<EditMode>(initial === EditMode.Unset ? EditMode.Simple : initial)

    watch(mode, (v) => {
        localStorage.setItem(STORAGE_KEY, v)
    })

    return { mode }
}

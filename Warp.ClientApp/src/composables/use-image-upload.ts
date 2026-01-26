import { fetchJson } from '../api/fetch-service'
import type { ApiError } from '../types/apis/api-error'


/** Handles paste events for image uploads. */
export async function handlePaste(
    event: ClipboardEvent,
    getEntryId: () => string | undefined,
    addLocalFiles?: (files: File[]) => void
) {
    try {
        if (!event)
            return false

        const typedEvent = event as ClipboardEventWithDetail
        const clipboardFiles = Array.from(event.clipboardData?.files || [])
        const detailFiles = typedEvent.detail?.files ?? []
        const files = clipboardFiles.length > 0 ? clipboardFiles : detailFiles
        if (files.length === 0)
            return false

        const entryId = getEntryId()
        if (!entryId)
            return false

        if (addLocalFiles)
            addLocalFiles(files)

        await uploadImages(entryId, files)
        return true
    } catch (err) {
        console.error(err)
        return false
    }
}


/** Initializes drag-and-drop and file input handlers for image uploads. */
export function initDropAreaHandlers(dropArea: HTMLElement, fileInput: HTMLInputElement, getEntryId: () => string | undefined) {
    if (!dropArea || !fileInput)
        return

    const eventManager = createEventListenerManager()
    const highlight = createHighlightController(dropArea)

    eventManager.add(dropArea, 'dragenter', (e: Event) => { preventDefaultAndStop(e); highlight.show() })
    eventManager.add(dropArea, 'dragover', (e: Event) => { preventDefaultAndStop(e); highlight.show() })
    eventManager.add(dropArea, 'dragleave', (e: Event) => { preventDefaultAndStop(e); highlight.hide() })
    eventManager.add(dropArea, 'drop', (e: Event) => { handleDrop(e as DragEvent, getEntryId, highlight) })
    eventManager.add(fileInput, 'change', (e: Event) => { handleFileInputChange(e, getEntryId) })

    return eventManager.removeAll
}


/** Uploads image files for a given entry ID. */
export async function uploadImages(entryId: string, files: File[]) {
    if (!entryId)
        throw new Error('entryId is required')

    const valid = files.filter(isValidImageFile)
    if (valid.length === 0)
        return null

    const form = new FormData()
    valid.forEach(f => form.append('Images', f, f.name))

    try {
        const json = await fetchJson(`/api/images/entry-id/${encodeURIComponent(entryId)}`, {
            method: 'POST',
            body: form
        })

        window.dispatchEvent(new Event(UPLOAD_FINISHED_EVENT))

        return json
    } catch (err) {
        const apiError = err as ApiError
        apiError.message = apiError.problem?.detail || apiError.message || 'Image upload failed'
        
        throw apiError
    }
}


/** Event name emitted when an image upload has finished. */
export const UPLOAD_FINISHED_EVENT = 'uploadFinished'


function createEventListenerManager() {
    const listeners: Array<{ target: EventTarget; type: string; handler: EventListenerOrEventListenerObject }> = []

    function add(target: EventTarget, type: string, handler: EventListenerOrEventListenerObject) {
        target.addEventListener(type, handler as EventListener)
        listeners.push({ target, type, handler })
    }

    function removeAll() {
        for (const listener of listeners)
            listener.target.removeEventListener(listener.type, listener.handler as EventListener)
    }

    return { add, removeAll }
}


function createHighlightController(dropArea: HTMLElement) {
    return {
        show: () => dropArea.classList.add('paste-area--highlighted'),
        hide: () => dropArea.classList.remove('paste-area--highlighted')
    }
}


async function handleDrop(e: DragEvent, getEntryId: () => string | undefined, highlight: ReturnType<typeof createHighlightController>) {
    e.preventDefault()
    e.stopPropagation()
    highlight.hide()

    const entryId = getEntryId()
    if (!entryId || !e.dataTransfer)
        return

    const files = Array.from(e.dataTransfer.files || [])
    try {
        await uploadImages(entryId, files)
    } catch (err) {
        console.error(err)
    }
}


async function handleFileInputChange(e: Event, getEntryId: () => string | undefined) {
    const entryId = getEntryId()
    if (!entryId)
        return

    const input = e.target as HTMLInputElement
    if (!input.files)
        return

    const files = Array.from(input.files)
    try {
        await uploadImages(entryId, files)
    } catch (err) {
        console.error(err)
    } finally {
        input.value = ''
    }
}


function isValidImageFile(file: File | null | undefined) {
    return !!file && !!file.type && file.type.startsWith('image/')
}


function preventDefaultAndStop(e: Event) {
    e.preventDefault()
    e.stopPropagation()
}


type ClipboardEventWithDetail = ClipboardEvent & { detail?: { files?: File[] } }

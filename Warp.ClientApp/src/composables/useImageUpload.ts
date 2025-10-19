import { ref } from 'vue'

const UPLOAD_FINISHED_EVENT = 'uploadFinished'


function isValidImageFile(file: File | null | undefined) {
    return !!file && !!file.type && file.type.startsWith('image/')
}


export async function uploadImages(entryId: string, files: File[]) {
    if (!entryId)
        throw new Error('entryId is required')

    const valid = files.filter(isValidImageFile)
    if (valid.length === 0)
        return null

    const form = new FormData()
    valid.forEach(f => form.append('Images', f, f.name))

    const response = await fetch(`/api/images/entry-id/${encodeURIComponent(entryId)}`, {
        method: 'POST',
        body: form,
        credentials: 'include'
    })

    if (!response.ok) {
        const text = await response.text().catch(() => '')
        const err = new Error(`Image upload failed: ${response.status} ${response.statusText} ${text}`)
        throw err
    }

    const json = await response.json()
    window.dispatchEvent(new Event(UPLOAD_FINISHED_EVENT))

    return json
}


export function initDropAreaHandlers(dropArea: HTMLElement, fileInput: HTMLInputElement, getEntryId: () => string | undefined) {
    if (!dropArea || !fileInput)
        return

    const preventDefault = (e: Event) => { e.preventDefault(); e.stopPropagation() }
    const toggleHighlight = (isHighlighted: boolean) => {
        dropArea.classList.toggle('paste-area--highlighted', isHighlighted)
    }

    const onDrop = async (e: DragEvent) => {
        preventDefault(e)
        toggleHighlight(false)
        const entryId = getEntryId()
        if (!entryId)
            return

        const dataTransfer = e.dataTransfer
        if (!dataTransfer)
            return

        const files = Array.from(dataTransfer.files || [])
        try {
            await uploadImages(entryId, files)
        } catch (err) {
            console.error(err)
        }
    }

    const onChange = async (e: Event) => {
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

    const listeners: Array<{ target: EventTarget; type: string; handler: EventListenerOrEventListenerObject }> = []

    function add(target: EventTarget, type: string, handler: EventListenerOrEventListenerObject) {
        target.addEventListener(type, handler as EventListener)
        listeners.push({ target, type, handler })
    }

    add(dropArea, 'dragenter', preventDefault)
    add(dropArea, 'dragover', preventDefault)
    add(dropArea, 'dragleave', preventDefault)
    add(dropArea, 'drop', preventDefault)

    // Wrap typed handlers into EventListener for DOM API
    add(dropArea, 'dragenter', (e: Event) => { preventDefault(e); toggleHighlight(true) })
    add(dropArea, 'dragover', (e: Event) => { preventDefault(e); toggleHighlight(true) })
    add(dropArea, 'dragleave', (e: Event) => { preventDefault(e); toggleHighlight(false) })
    add(dropArea, 'drop', (e: Event) => { onDrop(e as DragEvent) })

    add(fileInput, 'change', (e: Event) => { onChange(e) })

    return () => {
        for (const listener of listeners) {
            listener.target.removeEventListener(listener.type, listener.handler as EventListener)
        }
    }
}


export async function handlePaste(event: ClipboardEvent, getEntryId: () => string | undefined) {
    try {
        if (!event?.clipboardData)
            return false

        const files = Array.from(event.clipboardData.files || [])
        if (files.length === 0)
            return false

        const entryId = getEntryId()
        if (!entryId)
            return false

        await uploadImages(entryId, files)
        return true
    } catch (err) {
        console.error(err)
        return false
    }
}

export { UPLOAD_FINISHED_EVENT }

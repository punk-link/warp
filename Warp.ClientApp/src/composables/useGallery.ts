import { ref, readonly, watch, onBeforeUnmount, type Ref, computed } from 'vue'

export interface GalleryItemDraft {
    file: File
    url: string
    name: string
    type: string
    size: number
    addedAt: number
}


interface InternalStore {
    items: Ref<GalleryItemDraft[]>
    refs: number
}


interface UseGalleryOptions {
    maxFileBytes?: number
    maxFileCount?: number
    accept?: (file: File) => boolean
}


const DEFAULT_MAX_FILE_BYTES = 10 * 1024 * 1024 // 10MB
const DEFAULT_MAX_FILE_COUNT = 10


const stores = new Map<string, InternalStore>()


function createStore(): InternalStore {
    return { items: ref<GalleryItemDraft[]>([]), refs: 0 }
}


function getOrCreateStore(entryId: string): InternalStore {
    let store = stores.get(entryId)
    if (!store) {
        store = createStore()
        stores.set(entryId, store)
    }

    return store
}


function disposeStore(entryId: string) {
    const store = stores.get(entryId)
    if (!store) 
        return
    
    for (const it of store.items.value) {
        URL.revokeObjectURL(it.url)
    }

    stores.delete(entryId)
}


function defaultAccept(file: File) {
    return !!file.type && file.type.startsWith('image/')
}


export interface GalleryApi {
    items: Ref<GalleryItemDraft[]>
    count: Ref<number>
    totalBytes: Ref<number>
    addFiles: (list: FileList | File[] | null | undefined) => { added: number; rejected: number }
    remove: (index: number) => void
    clear: () => void
}


/**
 * Provides a reactive gallery of images keyed by an entry id. Multiple component instances
 * using the same entry id will share the same underlying store (in‑memory only).
 * Data is lost on full page reload which is acceptable for ephemeral drafts.
 */
export function useGallery(entryIdRef?: Ref<string | null | undefined>, options?: UseGalleryOptions): GalleryApi {
    const maxBytes = options?.maxFileBytes ?? DEFAULT_MAX_FILE_BYTES
    const maxCount = options?.maxFileCount ?? DEFAULT_MAX_FILE_COUNT
    const accept = options?.accept ?? defaultAccept

    const activeEntryId = ref<string | null>(entryIdRef ? entryIdRef.value || null : null)
    let store: InternalStore | null = activeEntryId.value ? getOrCreateStore(activeEntryId.value) : null
    if (store) 
        store.refs++

    const items = ref<GalleryItemDraft[]>(store ? store.items.value : [])

    function syncLocalToStore() {
        if (!store) { 
            items.value = []
            return 
        }

        items.value = store.items.value
    }

    function withStore<T>(fn: () => T): T | undefined {
        if (!store) 
            return undefined
        
        return fn()
    }

    function addFiles(list: FileList | File[] | null | undefined) {
        if (!list) 
            return { added: 0, rejected: 0 }

        if (!store) 
            return { added: 0, rejected: (list as any as File[]).length }

        const arr = Array.from(list as any as File[])
        let added = 0
        let rejected = 0
        for (const f of arr) {
            if (store.items.value.length >= maxCount) { 
                rejected += 1
                continue 
            }

            if (!accept(f)) { 
                rejected += 1
                continue 
            }

            if (f.size > maxBytes) { 
                rejected += 1
                continue 
            }

            const url = URL.createObjectURL(f)
            store.items.value.push({ file: f, url, name: f.name, type: f.type, size: f.size, addedAt: Date.now() })
            added += 1
        }
        
        syncLocalToStore()
        return { added, rejected }
    }

    function remove(index: number) {
        withStore(() => {
            const it = store!.items.value.splice(index, 1)[0]
            if (it) {
                try { URL.revokeObjectURL(it.url) } catch { }
            }
            syncLocalToStore()
        })
    }

    function clear() {
        withStore(() => {
            for (const it of store!.items.value) {
                try { URL.revokeObjectURL(it.url) } catch { }
            }
            store!.items.value = []
            syncLocalToStore()
        })
    }

    const count = computed(() => items.value.length)
    const totalBytes = computed(() => items.value.reduce((sum, it) => sum + it.size, 0))

    // React to external ref changes
    if (entryIdRef) {
        watch(entryIdRef, (val, prev) => {
            if (val === prev) return
            // decrement old
            if (store) {
                store.refs--
                if (store.refs <= 0) disposeStore(prev || '')
            }
            activeEntryId.value = val || null
            store = val ? getOrCreateStore(val) : null
            if (store) store.refs++
            syncLocalToStore()
        })
    }

    onBeforeUnmount(() => {
        if (store) {
            store.refs--
            if (store.refs <= 0 && activeEntryId.value) disposeStore(activeEntryId.value)
        }
    })

    return { items: readonly(items) as Ref<GalleryItemDraft[]>, count, totalBytes, addFiles, remove, clear }
}

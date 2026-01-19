import { ref, readonly, watch, onBeforeUnmount, type Ref, computed } from 'vue'
import type { GalleryItem } from '../types/galleries/gallery-item'
import type { LocalGalleryItem } from '../types/galleries/local-gallery-item'


interface InternalStore {
    items: Ref<GalleryItem[]>
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
const disposalTimers = new Map<string, number>()
const DISPOSE_DELAY_MS = 4000


function scheduleDispose(id: string) {
    if (!id) 
        return

    const existing = disposalTimers.get(id)
    if (existing) {
        clearTimeout(existing)
        disposalTimers.delete(id)
    }

    const timer = window.setTimeout(() => {
        const current = stores.get(id)
        if (current && current.refs <= 0)
            disposeStore(id)

        disposalTimers.delete(id)
    }, DISPOSE_DELAY_MS)
    disposalTimers.set(id, timer)
}


function cancelScheduledDispose(id: string | null | undefined) {
    if (!id) 
        return

    const timer = disposalTimers.get(id)
    if (timer) {
        clearTimeout(timer)
        disposalTimers.delete(id)
    }
}


function createStore(): InternalStore {
    return { 
        items: ref<GalleryItem[]>([]), 
        refs: 0 
    }
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
        if ((it as LocalGalleryItem).file)
            URL.revokeObjectURL(it.url)
    }

    stores.delete(entryId)
}


function defaultAccept(file: File) {
    return !!file.type && file.type.startsWith('image/')
}


export interface GalleryApi {
    items: Ref<GalleryItem[]>
    count: Ref<number>
    totalBytes: Ref<number>
    addFiles: (list: FileList | File[] | null | undefined) => { added: number; rejected: number }
    remove: (index: number) => void
    clear: () => void
    setServerImages: (urls: string[]) => void
}


export function useGallery(entryIdRef?: Ref<string | null | undefined>, options?: UseGalleryOptions): GalleryApi {
    const maxBytes = options?.maxFileBytes ?? DEFAULT_MAX_FILE_BYTES
    const maxCount = options?.maxFileCount ?? DEFAULT_MAX_FILE_COUNT
    const accept = options?.accept ?? defaultAccept

    const activeEntryId = ref<string | null>(entryIdRef ? entryIdRef.value || null : null)
    let store: InternalStore | null = activeEntryId.value ? getOrCreateStore(activeEntryId.value) : null
    if (store) 
        store.refs++

    const items = ref<GalleryItem[]>(store ? store.items.value : [])


    function syncLocalToStore() {
        if (!store) { 
            items.value = []
            return 
        }

        items.value = store.items.value
    }


    function withStore<T>(func: () => T): T | undefined {
        if (!store) 
            return undefined
        
        return func()
    }


    function addFiles(files: FileList | File[] | null | undefined) {
        if (!files) 
            return { added: 0, rejected: 0 }

        if (!store) 
            return { added: 0, rejected: (files as any as File[]).length }

        const fileArray = Array.from(files as any as File[])
        let added = 0
        let rejected = 0
        for (const file of fileArray) {
            if (store.items.value.length >= maxCount) { 
                rejected += 1
                continue 
            }

            if (!accept(file)) { 
                rejected += 1
                continue 
            }

            if (file.size > maxBytes) { 
                rejected += 1
                continue 
            }

            const url = URL.createObjectURL(file)
            const localItem: LocalGalleryItem = { kind: 'local', file: file, url, name: file.name, type: file.type, size: file.size, addedAt: Date.now() }
            store.items.value.push(localItem)
            added += 1
        }
        
        syncLocalToStore()
        return { added, rejected }
    }


    function remove(index: number) {
        withStore(() => {
            const item = store!.items.value.splice(index, 1)[0]
            if (item && item.kind === 'local')
                URL.revokeObjectURL(item.url)
            
            syncLocalToStore()
        })
    }


    function clear() {
        withStore(() => {
            for (const item of store!.items.value) {
                if (item.kind === 'local')
                    URL.revokeObjectURL(item.url)
            }

            store!.items.value = []
            syncLocalToStore()
        })
    }


    function setServerImages(urls: string[]) {
        return withStore(() => {
            for (const it of store!.items.value) {
                if (it.kind === 'local')
                    URL.revokeObjectURL(it.url)
            }

            store!.items.value = urls.map(u => ({
                kind: 'remote',
                url: u,
                name: (() => {
                    try {
                        const p = new URL(u).pathname
                        return p.substring(p.lastIndexOf('/') + 1) || u
                    } catch {
                        return u
                    }
                })(),
                type: '',
                size: 0,
                addedAt: Date.now()
            }))

            syncLocalToStore()
        })
    }


    const count = computed(() => items.value.length)
    const totalBytes = computed(() => items.value.reduce((sum, it) => sum + (it.size ?? 0), 0))

    if (entryIdRef) {
        watch(entryIdRef, (val, prev) => {
            if (val === prev) 
                return
            
            if (store) {
                store.refs--
                if (store.refs <= 0 && prev) 
                    scheduleDispose(prev)
            }

            activeEntryId.value = val || null
            store = val ? getOrCreateStore(val) : null
            if (store) 
                store.refs++

            cancelScheduledDispose(val)
            syncLocalToStore()
        })
    }
    

    onBeforeUnmount(() => {
        if (store) {
            store.refs--
            if (store.refs <= 0 && activeEntryId.value) 
                scheduleDispose(activeEntryId.value)
        }
    })

    return { items: readonly(items) as Ref<GalleryItem[]>, count, totalBytes, addFiles, remove, clear, setServerImages }
}

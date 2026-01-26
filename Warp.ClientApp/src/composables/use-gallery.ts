import { ref, readonly, watch, onBeforeUnmount, type Ref, computed } from 'vue'
import type { GalleryItem } from '../types/galleries/gallery-item'
import type { LocalGalleryItem } from '../types/galleries/local-gallery-item'
import { GalleryApi } from '../types/galleries/gallery-api'


const DEFAULT_MAX_FILE_BYTES = 10 * 1024 * 1024 // 10MB
const DEFAULT_MAX_FILE_COUNT = 10
const DISPOSE_DELAY_MS = 4000

const disposableTimers = new Map<string, number>()
const stores = new Map<string, InternalStore>()


export function useGallery(entryIdRef?: Ref<string | null | undefined>, options?: UseGalleryOptions): GalleryApi {
    const maxBytes = options?.maxFileBytes ?? DEFAULT_MAX_FILE_BYTES
    const maxCount = options?.maxFileCount ?? DEFAULT_MAX_FILE_COUNT
    const accept = options?.accept ?? defaultAccept

    const activeEntryId = ref<string | null>(entryIdRef ? entryIdRef.value || null : null)
    let store = connectToStore(activeEntryId.value)

    const items = ref<GalleryItem[]>(store ? store.items.value : [])

    const count = computed(() => items.value.length)
    const totalBytes = computed(() => items.value.reduce((sum, it) => sum + (it.size ?? 0), 0))

    if (entryIdRef) {
        watch(entryIdRef, (val, prev) => {
            if (val === prev)
                return

            disconnectFromStore(store, prev || null)

            activeEntryId.value = val || null
            store = connectToStore(activeEntryId.value)

            syncStoreToLocal(store, items)
        })
    }

    onBeforeUnmount(() => {
        disconnectFromStore(store, activeEntryId.value)
    })

    return {
        items: readonly(items) as Ref<GalleryItem[]>,
        count,
        totalBytes,
        addFiles: (files) => addFilesToStore(files, store, maxCount, maxBytes, accept, items),
        remove: (index) => removeFromStore(index, store, items),
        clear: () => clearStore(store, items),
        setServerImages: (urls) => setServerImagesToStore(urls, store, items)
    }
}


function addFilesToStore(
    files: FileList | File[] | null | undefined,
    store: InternalStore | null,
    maxCount: number,
    maxBytes: number,
    accept: (file: File) => boolean,
    localItems: Ref<GalleryItem[]>
): { added: number; rejected: number } {
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

        if (!isValidFile(file, maxBytes, accept)) {
            rejected += 1
            continue
        }

        const localItem = createLocalGalleryItem(file)
        store.items.value.push(localItem)
        added += 1
    }

    syncStoreToLocal(store, localItems)
    return { added, rejected }
}


function cancelScheduledDispose(id: string | null | undefined) {
    if (!id) 
        return

    const timer = disposableTimers.get(id)
    if (timer) {
        clearTimeout(timer)
        disposableTimers.delete(id)
    }
}


function clearStore(store: InternalStore | null, localItems: Ref<GalleryItem[]>) {
    if (!store)
        return

    revokeLocalUrls(store.items.value)
    store.items.value = []
    syncStoreToLocal(store, localItems)
}


function connectToStore(entryId: string | null): InternalStore | null {
    if (!entryId)
        return null

    const store = getOrCreateStore(entryId)
    store.refs++
    cancelScheduledDispose(entryId)
    
    return store
}


function createLocalGalleryItem(file: File): LocalGalleryItem {
    const url = URL.createObjectURL(file)
    return {
        kind: 'local',
        file: file,
        url,
        name: file.name,
        type: file.type,
        size: file.size,
        addedAt: Date.now()
    }
}


function createRemoteGalleryItem(url: string): GalleryItem {
    return {
        kind: 'remote',
        url: url,
        name: extractFileNameFromUrl(url),
        type: '',
        size: 0,
        addedAt: Date.now()
    }
}


function createStore(): InternalStore {
    return { 
        items: ref<GalleryItem[]>([]), 
        refs: 0 
    }
}


function defaultAccept(file: File) {
    return !!file.type && file.type.startsWith('image/')
}


function disconnectFromStore(store: InternalStore | null, entryId: string | null) {
    if (!store || !entryId)
        return

    store.refs--
    if (store.refs <= 0)
        scheduleDispose(entryId)
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


function extractFileNameFromUrl(url: string): string {
    try {
        const p = new URL(url).pathname
        return p.substring(p.lastIndexOf('/') + 1) || url
    } catch {
        return url
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


function isValidFile(file: File, maxBytes: number, accept: (file: File) => boolean): boolean {
    return accept(file) && file.size <= maxBytes
}


function removeFromStore(index: number, store: InternalStore | null, localItems: Ref<GalleryItem[]>) {
    if (!store)
        return

    const item = store.items.value.splice(index, 1)[0]
    if (item && item.kind === 'local')
        URL.revokeObjectURL(item.url)

    syncStoreToLocal(store, localItems)
}


function revokeLocalUrls(items: GalleryItem[]) {
    for (const item of items) {
        if (item.kind === 'local')
            URL.revokeObjectURL(item.url)
    }
}


function scheduleDispose(id: string) {
    if (!id) 
        return

    const existing = disposableTimers.get(id)
    if (existing) {
        clearTimeout(existing)
        disposableTimers.delete(id)
    }

    const timer = window.setTimeout(() => {
        const current = stores.get(id)
        if (current && current.refs <= 0)
            disposeStore(id)

        disposableTimers.delete(id)
    }, DISPOSE_DELAY_MS)
    disposableTimers.set(id, timer)
}


function setServerImagesToStore(urls: string[], store: InternalStore | null, localItems: Ref<GalleryItem[]>) {
    if (!store)
        return

    revokeLocalUrls(store.items.value)
    store.items.value = urls.map(createRemoteGalleryItem)
    syncStoreToLocal(store, localItems)
}


function syncStoreToLocal(store: InternalStore | null, localItems: Ref<GalleryItem[]>) {
    localItems.value = store ? store.items.value : []
}


interface InternalStore {
    items: Ref<GalleryItem[]>
    refs: number
}


interface UseGalleryOptions {
    maxFileBytes?: number
    maxFileCount?: number
    accept?: (file: File) => boolean
}

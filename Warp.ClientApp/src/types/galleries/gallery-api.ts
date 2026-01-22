import { Ref } from "vue";
import { GalleryItem } from "./gallery-item";


/** API for interacting with a gallery of images. */
export interface GalleryApi {
    items: Ref<GalleryItem[]>
    count: Ref<number>
    totalBytes: Ref<number>
    addFiles: (list: FileList | File[] | null | undefined) => { added: number; rejected: number }
    remove: (index: number) => void
    clear: () => void
    setServerImages: (urls: string[]) => void
}
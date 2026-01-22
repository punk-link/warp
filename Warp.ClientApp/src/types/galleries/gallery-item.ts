import { LocalGalleryItem } from "./local-gallery-item";
import { RemoteGalleryItem } from "./remote-gallery-item";


/** Represents an item in the gallery, which can be either a local or remote image. */
export type GalleryItem = LocalGalleryItem | RemoteGalleryItem
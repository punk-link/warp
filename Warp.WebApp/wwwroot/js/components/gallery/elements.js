import { dom } from '/js/utils/ui-core.js';


export const elements = {
    getGallery: () => dom.query('.gallery'),
    getUploadContainer: () => dom.get('empty-image-container'),
    getImageContainer: (element) => ({
        container: element,
        deleteButton: element.querySelector('.delete-image-button'),
        imageId: element.dataset.imageId,
        icon: element.querySelector('i'),
        image: element.querySelector('img')
    })
};
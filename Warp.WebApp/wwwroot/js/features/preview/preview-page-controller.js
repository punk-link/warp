import { entryApi } from '/js/api/entry-api.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { galleryViewer } from '/js/components/gallery/viewer.js';
import { http } from '/js/services/http/client.js';
import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { copyUrl } from '/js/utils/clipboard.js';
import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { initializeCountdown } from '/js/components/countdown.js'; 
import { BasePageController } from '/js/features/base-page-controller.js';


export class PreviewPageController extends BasePageController {
    constructor(elements) {
        super(elements);
    }
    

    async initialize(entryId) {
        this.executeWithLoadingIndicator(async () => {
                try {
                this.#initRoamingImage();

                const entryResult = await entryApi.get(entryId);
                if (entryResult.isFailure) {
                    displayError(entryResult.error);
                    return;
                }

                const entry = entryResult.value;
                this.#setupEventHandlers(entry.id);

                let isEditable = false;
                const isEditableResult = await entryApi.isEditable(entry.id);
                if (isEditableResult.isSuccess)
                    isEditable = isEditableResult.value;

                await this.#updateUIWithData(entry, isEditable);
            } catch (error) {
                this.captureException(error, 'Failed to initialize preview page', 'initialize');
            }
        });
    }


    async #attachImageContainersToGallery(gallery, images) {
        if (images.length === 0)
            return;

        for (const image of images) {
            const response = await http.get(image.url + '/partial/read-only');
            if (!response.ok) {
                displayError(response.error);
                continue;
            }

            const imageContainer = await response.text();
            gallery.insertAdjacentHTML('beforeend', imageContainer);
        }

        galleryViewer.init();
        gallery.classList.toggle(CSS_CLASSES.HIDDEN, false);
    }


    async #handleCopyLink(entryId) {
        const entryUrl = `${window.location.origin}${ROUTES.ENTRY}/${entryId}`;
        const success = await copyUrl(entryUrl);

        if (success) {
            const editButton = this.elements.getActionButtons().edit;
            if (editButton) {
                uiState.toggleClasses(editButton, {
                    remove: [CSS_CLASSES.HIDDEN],
                    add: [CSS_CLASSES.ANIMATE]
                });
            }

            const { created, copied } = this.elements.getServiceMessages();
            if (created && copied) {
                uiState.toggleClasses(created, { add: [CSS_CLASSES.HIDDEN] });
                uiState.toggleClasses(copied, {
                    remove: [CSS_CLASSES.HIDDEN],
                    add: [CSS_CLASSES.ANIMATE, CSS_CLASSES.ANIMATE_SLOW_OUT]
                });

                setTimeout(() => {
                    uiState.toggleClasses(copied, {
                        add: [CSS_CLASSES.HIDDEN],
                        remove: [CSS_CLASSES.ANIMATE]
                    });
                    uiState.toggleClasses(created, {
                        remove: [CSS_CLASSES.HIDDEN],
                        add: [CSS_CLASSES.ANIMATE_SLOW]
                    });
                }, 4000);
            }
        }
    }


    async #handleCopy(entryId) {
        this.executeWithLoadingIndicator(async () => {
            const result = await entryApi.copy(entryId);
            if (result.isSuccess)
                redirectTo(ROUTES.ROOT, { id: result.value.id });
            else
                displayError(result.error);
        });
    }


    #handleEdit(entryId) {
        this.executeWithLoadingIndicator(() => {
            redirectTo(ROUTES.ROOT, { id: entryId });
        });
    }


    async #handleDelete(entryId) {
        this.executeWithLoadingIndicator(async () => {
            const result = await entryApi.delete(entryId);
            if (result.isSuccess)
                redirectTo(ROUTES.DELETED);
            else
                displayError(result.error);
        });
    }


    #initRoamingImage() {
        try {
            const roamingImage = this.elements.getRoamingImage();
            if (roamingImage) 
                animateBackgroundImage(roamingImage);
        } catch (error) {
            console.error('Error initializing roaming image:', error);
        }
    }


    #setupEventHandlers(entryId) {
        const { copyLink: copyLinkButton,edit: editButton, editCopy: editCopyButton, delete: deleteButton } = this.elements.getActionButtons();
        
        copyLinkButton.addEventListener('click', () => this.#handleCopyLink(entryId));
        editCopyButton.addEventListener('click', () => this.#handleCopy(entryId));
        deleteButton.addEventListener('click', () => this.#handleDelete(entryId));
        editButton.addEventListener('click', () => this.#handleEdit(entryId));
    }


    async #updateUIWithData(entry, isEditable) {
        initializeCountdown(new Date(entry.expiresAt));

        const textContent = this.elements.getTextContentElement();
        textContent.textContent = entry.textContent;

        const gallery = this.elements.getGallery();
        await this.#attachImageContainersToGallery(gallery, entry.images);

        const editButton = this.elements.getEditEntryButton();
        editButton.classList.toggle(CSS_CLASSES.HIDDEN, !isEditable);
    }
}
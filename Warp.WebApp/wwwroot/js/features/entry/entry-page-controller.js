import { creatorApi } from '/js/api/creator-api.js';
import { entryApi } from '/js/api/entry-api.js';
import { preview } from '/js/components/gallery/preview.js';
import { galleryViewer } from '/js/components/gallery/viewer.js';
import { http } from '/js/services/http/client.js';
import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { copyUrl } from '/js/utils/clipboard.js';
import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { initializeCountdown } from '/js/components/countdown.js';
import { modal } from '/js/components/modal.js';
import { BasePageController } from '/js/features/base-page-controller.js';


export class EntryPageController extends BasePageController {
    constructor(elements) {
        super(elements);
    }
    

    async initialize(entryId) {
        this.executeWithLoadingIndicator(async () => {
            try {
                this.initRoamingImage();

                await creatorApi.getOrSet();
                const entryResult = await entryApi.get(entryId);
                if (entryResult.isFailure) {
                    this.displayError(entryResult.error);
                    return;
                }

                const entry = entryResult.value;
                this.#setupEventHandlers(entry.id);
                await this.#updateUIWithData(entry);
            } catch (error) {
                this.captureException(error, 'Failed to initialize entry page', 'initialize');
            }
        });
    }


    async #attachImageContainersToGallery(gallery, images) {
        if (images.length === 0)
            return;

        for (const image of images) {
            const response = await http.get(image.url + '/partial/read-only');
            if (!response.ok) {
                this.displayError(response.error);
                continue;
            }

            const imageContainerHtml = await response.text();
            preview.animateReadOnlyContainer(imageContainerHtml, gallery);
        }

        galleryViewer.init();
        gallery.classList.toggle(CSS_CLASSES.HIDDEN, false);
    }


    async #handleCopyLink(entryId) {
        const entryUrl = `${window.location.origin}${ROUTES.ENTRY}/${entryId}`;
        const success = await copyUrl(entryUrl);

        if (success) {
            // Show success feedback if we have service messages
            const serviceMessages = this.elements.getServiceMessages?.();
            if (serviceMessages?.copied) {
                uiState.toggleClasses(serviceMessages.copied, {
                    remove: [CSS_CLASSES.HIDDEN],
                    add: [CSS_CLASSES.ANIMATE, CSS_CLASSES.ANIMATE_SLOW_OUT]
                });

                setTimeout(() => {
                    uiState.toggleClasses(serviceMessages.copied, {
                        add: [CSS_CLASSES.HIDDEN],
                        remove: [CSS_CLASSES.ANIMATE]
                    });
                }, 3000);
            }
        }
    }


    #handleClose() {
        redirectTo(ROUTES.ROOT);
    }


    async #handleReport(entryId) {
        this.executeWithLoadingIndicator(async () => {
            const response = await http.post(ROUTES.API.ENTRIES.REPORT(entryId));
            if (response.status === 204)
                redirectTo(ROUTES.ROOT);
            else
                this.displayError({ message: 'Failed to report entry' });
        });
    }


    #setupEventHandlers(entryId) {
        const { close: closeButton, copyLink: copyLinkButton, showReportModalButton } = this.elements.getActionButtons();
        const { reportModal, reportButton, cancelButton } = this.elements.getModalElements();
        
        closeButton.addEventListener('click', () => this.#handleClose());
        copyLinkButton.addEventListener('click', () => this.#handleCopyLink(entryId));

        const reportModalControl = modal.create(reportModal);

        showReportModalButton.addEventListener('click', () => {
            reportModalControl.show();
        });

        if (cancelButton) {
            cancelButton.addEventListener('click', () => {
                reportModalControl.hide();
            });
        }

        if (reportButton) {
            reportButton.addEventListener('click', () => {
                reportModalControl.hide();
                this.#handleReport(entryId);
            });
        }
    }


    async #updateUIWithData(entry) {
        initializeCountdown(new Date(entry.expiresAt));

        const textContent = this.elements.getTextContentElement();
        textContent.textContent = entry.textContent;

        const viewCountElement = this.elements.getViewCountElement();
        viewCountElement.textContent = entry.viewCount || 0;

        const gallery = this.elements.getGallery();
        await this.#attachImageContainersToGallery(gallery, entry.images);
    }
}
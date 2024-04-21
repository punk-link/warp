const CLASS_FADE = 'fade';
const CLASS_SHOW = 'show';
const DISMISS_SELECTOR = '[data-dismiss="modal"]';
const TOGGLE_SELECTOR = '[data-toggle="modal"]';


class Modal {
    constructor(modalElement) {
        this.modalElement = modalElement;
        this.#registerDismissEvent();

        this.isShown = true;
        this.hide();
    }


    hide() {
        this.modalElement.classList.remove(CLASS_SHOW);
        this.modalElement.classList.add(CLASS_FADE);

        this.isShown = false;
    }


    show() {
        this.modalElement.classList.remove(CLASS_FADE);
        this.modalElement.classList.add(CLASS_SHOW);

        this.isShown = true;
    }


    toggle() {
        if (this.isShown) 
            this.hide();
        else 
            this.show();
    }


    #registerDismissEvent() {
        let dismissElement = this.modalElement.querySelector(DISMISS_SELECTOR);
        dismissElement.addEventListener('click', () => this.hide());
    }
 }


document.addEventListener('DOMContentLoaded', () => {
    let toggleButtons = document.querySelectorAll(TOGGLE_SELECTOR);

    toggleButtons.forEach(button => {
        let modalElement = document.querySelector(button.dataset.target);
        let modal = new Modal(modalElement);

        button.addEventListener('click', () => {
            modal.toggle();
        });
    });
});
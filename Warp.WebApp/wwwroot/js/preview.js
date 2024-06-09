import { copyUrl } from '/js/functions/copier.js';

export function addPreviewEvents() {
    let copyLinkButton = document.getElementById('copy-link-button');
    let editButton = document.getElementById('edit-button');

    copyLinkButton.addEventListener('click', function () {
        copyUrl();

        editButton.classList.remove('d-none');
        editButton.classList.add('catchy-fade-in');
    });
}
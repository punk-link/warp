import { copyUrl } from '/js/functions/copier.js';
import Countdown from '/js/components/countdown.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';
import { makeHttpRequest, DELETE } from '/js/functions/http-client.js';

async function deleteEntry(entryId) {
    let response = await makeHttpRequest(`/api/entries/${entryId}`, DELETE);

    if (response.ok)
        location.href = '/deleted';

    if (!(response.ok && response.redirected)) {
        let problemDetails = await response.json();
        let url = '/error?details=' + encodeURIComponent(JSON.stringify(problemDetails));
        
        location.href = url;
    }
}


export function addPreviewEvents(entryId, expirationDate) {
    let backgroundImageContainer = document.getElementById('roaming-image');
    repositionBackgroundImage(backgroundImageContainer);

    let countdownElement = document.getElementsByClassName('countdown')[0];
    let countdown = new Countdown(countdownElement, expirationDate);

    let copyLinkButton = document.getElementById('copy-link-button');
    let editButton = document.getElementById('edit-button');
    let deleteButton = document.getElementById('delete-button');

    copyLinkButton.addEventListener('click', function () {
        copyUrl(window.location.origin + /entry/ + entryId);

        editButton.classList.remove('d-none');
        editButton.classList.add('catchy-fade-in');
    });

    editButton.onclick = () => location.href = '/?id=' + entryId;

    deleteButton.onclick = async () => await deleteEntry(entryId);

    Fancybox.bind("[data-fancybox]", {
        caption: function (fancybox, slide) {
            return slide.thumbEl?.alt || "";
        }
    });
}
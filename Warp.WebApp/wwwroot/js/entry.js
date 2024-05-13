import Countdown from '/js/components/countdown.js';
import { copyUrl } from '/js/functions/copier.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';


async function report(id) {
    let response = await fetch('/api/reports/' + id, {
        method: 'POST'
    });

    if (response.status === 204)
        location.href = '/';
}


export function addEntryEvents(entryId, expirationDate) {
    let backgroundImageContainer = document.getElementById('roaming-image');
    repositionBackgroundImage(backgroundImageContainer);

    let countdownElement = document.getElementsByClassName('countdown')[0];
    let countdown = new Countdown(countdownElement, expirationDate);

    let copyButton = document.getElementById('copy-url-button');
    copyButton.onclick = () => copyUrl();

    let pageCloseButton = document.getElementById('page-close-button');
    pageCloseButton.onclick = () => location.href = '/';

    let reportButton = document.getElementById('report-button');
    reportButton.onclick = async () => await report(entryId);
}
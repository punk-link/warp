import Countdown from '/js/components/countdown.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';


async function report(id) {
    let response = await fetch('/api/entries/' + id + '/report', {
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

    let pageCloseButton = document.getElementById('page-close-button');
    pageCloseButton.onclick = () => location.href = '/';

    let reportButton = document.getElementById('report-button');
    reportButton.onclick = async () => await report(entryId);

    Fancybox.bind("[data-fancybox]", {
        caption: function (fancybox, slide) {
            return slide.thumbEl?.alt || "";
        }
    });
}
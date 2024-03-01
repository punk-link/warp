import { attachModalAction, showModal } from './modules/modal.js';


function copyUrl(copyButton) {
    try {
        document.body.style.cursor = 'wait';
        navigator.clipboard.writeText(window.location.href);

        let copiedCaption = copyButton.getElementsByClassName('secondary-caption')[0];
        copiedCaption.style.display = 'block';
    } finally {
        document.body.style.cursor = 'auto';
    }
}


async function report(id) {
    let response = await fetch('/api/reports/' + id, {
        method: 'POST'
    });

    if (response.status === 204)
        location.href = '/';
}


export function addEntryEvents(entryId) {
    let copyButton = document.getElementById('copy-url-button');
    copyButton.onclick = () => copyUrl(copyButton);

    attachModalAction(() => report(entryId));

    let reportButton = document.getElementById('report-button');
    reportButton.onclick = (e) => showModal(e);
}
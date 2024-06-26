import { copyUrl } from '/js/functions/copier.js';
import { navigateToErrorPage } from '/js/functions/error-interceptor.js';

async function deleteEntry(entryId) {
    let responce = await fetch('/api/entry', {
        method: 'DELETE',
        body: JSON.stringify({ id: entryId }),
        headers: {
            'Accept': 'application/json; charset=utf-8',
            'Content-Type': 'application/json; charset=utf-8'
        },
    });

    if (responce.ok)
        location.href = '/deleted';

    if (!(responce.ok && responce.redirected))
        navigateToErrorPage(responce.body);
}


export function addPreviewEvents(entryId) {
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
    
}
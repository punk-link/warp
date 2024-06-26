import { copyUrl } from '/js/functions/copier.js';
import { makeHttpRequest, POST, DELETE } from '/js/functions/http-client.js';

async function deleteEntry(entryId) {   
    let responce = await makeHttpRequest('/api/entry', DELETE, { id: entryId });

    if (responce.ok)
        location.href = '/deleted';

    if (!(responce.ok && responce.redirected))
        await makeHttpRequest('/error', POST, { problemDetails: responce.body });
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
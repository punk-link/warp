import { copyUrl } from '/js/functions/copier.js';

async function deleteEntry(id) {
    let responce = await fetch('/api/entry/delete', {
        method: 'DELETE',
        body: id
    });
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
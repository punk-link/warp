function appendImage(blob) {
    let image = document.createElement('img');
    image.src = URL.createObjectURL(blob);

    let imageContainer = document.getElementById('image-container');
    imageContainer.append(image);

    let textArea = document.getElementById('warp-content-textarea');
    textArea.style.display = 'none';
}


function cancelModal() {
    let modal = document.getElementsByClassName('modal-background')[0];
    modal.style.display = 'none';
}


function copyUrl() {
    try {
        document.body.style.cursor = 'wait';
        navigator.clipboard.writeText(window.location.href);
    } finally {
        document.body.style.cursor = 'auto';
    }
}


async function pasteImages() {
    try {
        await navigator.permissions.query({ name: 'clipboard-read' });

        let clipboardItems = await navigator.clipboard.read();
        for (let item of clipboardItems) {
            let imageTypes = item.types.filter(type => type.startsWith('image/'));
            for (let type of imageTypes) {
                let blob = await item.getType(type);
                appendImage(blob);
            }
        }
    } catch(err) {
        console.error(err);
    }
}


async function report(id) {
    let result = await fetch('/api/reports/' + id, {
        method: 'POST'
    });

    if (result.status === 204)
        location.href = '/';
}


function showModal(e) {
    e.stopImmediatePropagation();

    let modal = document.getElementsByClassName('modal-background')[0];
    if (modal === null || modal === undefined) 
        return;

    let modalWindow = document.getElementsByClassName('modal-window')[0];
    document.addEventListener('click', e1 => {
        if (!modalWindow.contains(e1.target)) 
            cancelModal();
    });

    let modalCancel = document.getElementById('modal-cancel');
    modalCancel.onclick = () => cancelModal();

    modal.style.display = 'flex';
}


document.addEventListener('DOMContentLoaded', () => {
});
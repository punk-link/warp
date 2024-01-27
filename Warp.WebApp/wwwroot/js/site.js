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


async function report(id) {
    let response = await fetch('/api/reports/' + id, {
        method: 'POST'
    });

    if (response.status === 204)
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
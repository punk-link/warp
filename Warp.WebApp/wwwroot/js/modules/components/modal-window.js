function cancelModal() {
    let modal = document.getElementsByClassName('modal-background')[0];
    modal.style.display = 'none';
}


export function attachModalAction(func) {
    let modalAction = document.getElementById('modal-action');
    modalAction.onclick = () => func();
}


export function showModal(e) {
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
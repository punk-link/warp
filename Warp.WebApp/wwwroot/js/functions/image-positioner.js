export function repositionBackgroundImage(element) {
    let randomY = Math.floor(Math.random() * 100);
    element.style.top = randomY + 'vh';
    
    let randomX = Math.floor(Math.random() * 100);
    element.style.left = randomX + 'vw';

    element.classList.remove('d-none');
    element.classList.add('slow-fade-in');
}
export function copyUrl(url) {
    try {
        document.body.style.cursor = 'wait';
        let href = url ? url : window.location.href;
        navigator.clipboard.writeText(href);
    } finally {
        document.body.style.cursor = 'auto';
    }
}
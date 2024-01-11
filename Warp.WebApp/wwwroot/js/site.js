function copyUrl() {
    try {
        document.body.style.cursor = 'wait';
        navigator.clipboard.writeText(window.location.href);
    } finally {
        document.body.style.cursor = 'auto';
    }
}
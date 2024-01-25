function addDropAreaEvents() {
    let dropArea = document.getElementsByClassName('form-vertical')[0];
    
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, function (e) {
            e.preventDefault();
            e.stopPropagation();
        }, false);
    })

    dropArea.addEventListener('drop', function (e) {
        let dt = e.dataTransfer;
        let files = dt.files;

        handleFiles(files);
    }, false);
}



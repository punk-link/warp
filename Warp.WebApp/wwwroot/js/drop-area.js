function addDropAreaEvents() {
    let dropArea = document.getElementsByClassName('drop-area')[0];
    
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, function (e) {
            e.preventDefault();
            e.stopPropagation();
        });
    });

    ['dragenter', 'dragover'].forEach(eventName => {
        dropArea.addEventListener(eventName, function () {
            dropArea.classList.add('highlighted');
        });
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, function () {
            dropArea.classList.remove('highlighted');
        });
    });

    dropArea.addEventListener('drop', processDrop);

    let fileInput = document.getElementById('file');
    fileInput.onchange = (e) => handleFiles(e.files);
}


function appendImage(file) {
    let image = document.createElement('img');
    image.src = URL.createObjectURL(file);

    let gallery = document.getElementsByClassName('gallery')[0];
    gallery.append(image);
}


async function pasteImages() {
    try {
        await navigator.permissions.query({ name: 'clipboard-read' });

        let clipboardItems = await navigator.clipboard.read();
        for (let item of clipboardItems) {
            let imageTypes = item.types
                .filter(type => type.startsWith('image/'));
            
            let files = [];
            for (let type of imageTypes) {
                let blob = await item.getType(type);
                files.push(blob);
            }
            
            processFiles(files);
        }
    } catch(err) {
        console.error(err);
    }
}


function processDrop(e) {
    let transfer = e.dataTransfer;
    let files = transfer.files;

    processFiles(files);
}


function processFiles(files) {
    ([...files]).forEach(file => {
        appendImage(file);

        let form = document.forms[0];
        form.addEventListener('submit', (e) => {
            e.preventDefault();

            let formData = new FormData(form);
            alert(JSON.stringify(formData));
        });
    });
}
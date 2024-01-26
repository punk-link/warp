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


function appendImage(files) {
    let gallery = document.getElementsByClassName('gallery')[0];

    files.forEach(file => {
        let image = document.createElement('img');
        image.src = URL.createObjectURL(file);

        gallery.append(image);
    });
}


function appendPreview(files) {
    let gallery = document.getElementsByClassName('gallery')[0];

    files.forEach(file => {
        let reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onloadend = function () {
            let image = document.createElement('img');
            img.src = reader.result;

            gallery.append(image);
        }
    });
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
            
            //appendImage(files);
            uploadFiles(files);
            appendPreview(files);
        }
    } catch(err) {
        console.error(err);
    }
}


function processDrop(e) {
    let transfer = e.dataTransfer;
    let fileList = transfer.files;
    let files = Array.from(fileList)
    
    //appendImage(files);
    uploadFiles(files);
    appendPreview(files);
}


function uploadFiles(files) {
    document.forms[0].addEventListener('submit', (e) => {
        e.preventDefault();

        let form = document.forms[0];
        let formData = new FormData(form);

        files.forEach(file => {
            formData.append('Images', file, file.name);
        });

        alert(JSON.stringify(Array.from(formData)));
    });
}
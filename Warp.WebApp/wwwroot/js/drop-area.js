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

    dropArea.addEventListener('drop', async (e) => await dropImages(e));

    let fileInput = document.getElementById('file');
    fileInput.onchange = (e) => {
        let files = Array.from(e.target.files)
        uploadFiles(files);
    };

    let uploadButton = document.getElementById('upload-button');
    uploadButton.onclick = () => fileInput.click();
}


function appendPreview(files, uploadResults) {
    let gallery = document.getElementsByClassName('upload-gallery')[0];
    let imageContainer = document.getElementsByClassName('image-container')[0];

    files.forEach(file => {
        let reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onloadend = function () {
            // TODO: add classes for image scaling

            let imageWrapper = document.createElement('div');
            imageWrapper.classList.add('image-wrapper');
            
            let image = document.createElement('img');
            image.src = reader.result;

            if (uploadResults[file.name]) {
                let input = getInputToImageId(uploadResults[file.name]);
                imageContainer.append(input);
                
                let uploadedIcon = getImageUploadedIcon();
                imageWrapper.append(uploadedIcon);
            }

            imageWrapper.append(image);
            gallery.append(imageWrapper);
        }
    });
}


function getImageUploadedIcon() {
    let uploadedIcon = document.createElement('i');
    uploadedIcon.classList.add('icofont-cloud-upload');

    let iconWrapper = document.createElement('div');
    iconWrapper.classList.add('icon-wrapper');
    iconWrapper.classList.add('flex-container');
    iconWrapper.classList.add('justify-center');
    iconWrapper.classList.add('align-center');

    iconWrapper.append(uploadedIcon);

    return iconWrapper;
}


function getInputToImageId(id) {
    let input = document.createElement('input');
    input.style.display = 'none';
    input.name = 'ImageIds';
    input.type = 'text';
    input.value = id;

    return input;
}


async function pasteImages() {
    try {
        await navigator.permissions.query({ name: 'clipboard-read' });

        let clipboardItems = await navigator.clipboard.read();
        let files = [];
        for (let item of clipboardItems) {
            let imageTypes = item.types
                .filter(type => type.startsWith('image/'));
            
            for (let type of imageTypes) {
                let blob = await item.getType(type);
                files.push(blob);
            }
        }
        
        uploadFiles(files);
    } catch(err) {
        console.error(err);
    }
}


function dropImages(e) {
    let transfer = e.dataTransfer;
    let fileList = transfer.files;
    let files = Array.from(fileList)
    
    uploadFiles(files);
}


async function uploadFiles(files) {
    let formData = new FormData();
    files.forEach(file => {
        formData.append('Images', file, file.name);
    });

    let response = await fetch('/api/images/', {
        method: 'POST',
        body: formData
    });

    if (response.status !== 200) {
        console.error(response.status, response.statusText);
        return;
    }
        
    let responseContent = await response.json();
    appendPreview(files, responseContent);
}
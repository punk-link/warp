import { makeHttpRequest, POST } from '../functions/http-client.js';


function appendPreview(files, imageContainer) {
    let gallery = document.getElementsByClassName('upload-gallery')[0];

    files.forEach(file => {
        let reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onloadend = function () {
            var parser = new DOMParser();
            let doc = parser.parseFromString(imageContainer, 'text/html');
            let containerElement = doc.getElementsByClassName('image-container')[0];

            containerElement.querySelector('img')
                .remove();

            let newImage = document.createElement('img');
            newImage.src = reader.result;

            containerElement.prepend(newImage);
            gallery.prepend(containerElement);
        }
    });
}


function dropImages(e) {
    let transfer = e.dataTransfer;
    let fileList = transfer.files;
    let files = Array.from(fileList)
    
    uploadImages(files);
}


async function uploadImages(files) {
    let formData = new FormData();
    files.forEach(file => {
        formData.append('Images', file, file.name);
    });

    let response = await fetch('/api/images/', {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        console.error(response.status, response.statusText);
        return;
    }
        
    let imageContainer = await response.text();
    appendPreview(files, imageContainer);
}


export function addDropAreaEvents(dropArea, fileInput, uploadButton) {
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

    fileInput.onchange = (e) => {
        let files = Array.from(e.target.files)
        uploadImages(files);
    };

    uploadButton.onclick = () => fileInput.click();
}


export async function pasteImages() {
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
        
        uploadImages(files);
    } catch(err) {
        console.error(err);
    }
}
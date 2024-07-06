import { uploadFinishedEvent } from '../events/events.js';
import { makeHttpRequest, POST } from '../functions/http-client.js';


function addImageDeleteEvent(containerElement) {
    let deleteButton = containerElement.querySelector('.delete-image-button');
    deleteButton.onclick = (e) => deleteImage(e);

    return containerElement;
}


function deleteImage(e) {
    e.target.closest('.image-container').remove();
 }


async function dropImages(e) {
    let transfer = e.dataTransfer;
    let fileList = transfer.files;
    let files = Array.from(fileList)
    
    await uploadImages(files);
}


function getImageContainerElement(imageContainer) {
    let domParser = new DOMParser();
    let doc = domParser.parseFromString(imageContainer, 'text/html');

    return doc.getElementsByClassName('image-container')[0];
}


function renderPreview(files, imageContainer) {
    let gallery = document.getElementsByClassName('upload-gallery')[0];

    files.forEach(file => {
        let reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onloadend = function () {
            let containerElement = getImageContainerElement(imageContainer);
            containerElement = replaceImagePreview(containerElement, reader);
            containerElement = addImageDeleteEvent(containerElement);

            gallery.prepend(containerElement);
        }
    });

    document.dispatchEvent(uploadFinishedEvent);
}


function replaceImagePreview(containerElement, reader) {
    let newImage = document.createElement('img');
    newImage.src = reader.result;
    
    containerElement.querySelector('img').remove();
    containerElement.prepend(newImage);

    return containerElement;
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
    renderPreview(files, imageContainer);
}


export function addDropAreaEvents(dropArea, fileInput, uploadButton) {
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, (e) => {
                e.preventDefault();
                e.stopPropagation();
            });
    });

    ['dragenter', 'dragover'].forEach(eventName => {
        dropArea.addEventListener(eventName, () => {
                dropArea.classList.add('highlighted');
            });
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, () => {
                dropArea.classList.remove('highlighted');
            });
    });

    dropArea.addEventListener('drop', async (e) => await dropImages(e));

    fileInput.onchange = async (e) => {
        let files = Array.from(e.target.files)
        await uploadImages(files);
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
        
        await uploadImages(files);
    } catch (err) {
        console.error(err);
    }
}
import { uploadFinishedEvent } from '../events/events.js';
import { makeHttpRequest, DELETE, POST } from '../functions/http-client.js';


const PLUS_ICON = 'icofont-plus';
const CLOCK_ICON = 'icofont-clock-time';


function addImageDeleteEvent(entryId, containerElement) {
    let deleteButton = containerElement.querySelector('.delete-image-button');
    deleteButton.onclick = (e) => deleteImage(e, entryId);

    return containerElement;
}


async function deleteImage(e, entryId) {
    let imageId = e.target.closest('.image-container').dataset.imageId;

    let response = await makeHttpRequest(`/api/images/entry-id/${entryId}/image-id/${imageId}`, DELETE);
    if (response.ok)
        e.target.closest('.image-container').remove();
 }


async function dropImages(e) {
    let transfer = e.dataTransfer;
    let fileList = transfer.files;
    let files = Array.from(fileList)
    
    await uploadImages(files);
}


function getImageContainerElement(uploadResult) {
    let domParser = new DOMParser();
    let doc = domParser.parseFromString(uploadResult, 'text/html');

    return doc.getElementsByClassName('image-container')[0];
}


function renderPreview(entryId, files, uploadResults) {
    let gallery = document.getElementsByClassName('upload-gallery')[0];

    files.forEach(file => {
        let reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onloadend = function () {
            let uploadResult = uploadResults[file.name];
            if (uploadResult === undefined) 
                return;

            let problemDetails = tryGetProblemDetails(uploadResult);
            if (problemDetails !== null) {
                console.error(`Error: ${problemDetails.title} - ${problemDetails.detail}`);
                return;
            }

            let containerElement = getImageContainerElement(uploadResult);
            containerElement = replaceImagePreview(containerElement, reader);
            containerElement = addImageDeleteEvent(entryId, containerElement);

            containerElement.classList.add('d-none');
            gallery.prepend(containerElement);

            containerElement.classList.remove('d-none');
            containerElement.classList.add('catchy-fade-in');
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


function toggleImageInUseStatus(element, removedState, addedState) {
    let icon = element.getElementsByTagName('i')[0];
    element.classList.add('d-none');

    icon.classList.remove(removedState);
    icon.classList.add(addedState);

    element.classList.remove('d-none');
    element.classList.add('catchy-fade-in');
}


function tryGetProblemDetails(uploadResult) {
    try {
        let parsedObject = JSON.parse(uploadResult);
        if (typeof parsedObject !== 'object' || parsedObject === null) 
            return null;

        if (!parsedObject.hasOwnProperty('title') || !parsedObject.hasOwnProperty('detail')) 
            return null;

        return parsedObject;
    } catch (e) {
        return null;
    }
}


function setImageInUseStatus(element) {
    toggleImageInUseStatus(element, PLUS_ICON, CLOCK_ICON);
}


function unsetImageInUseStatus(element) {
    toggleImageInUseStatus(element, CLOCK_ICON, PLUS_ICON);
}


async function uploadImages(entryId, files) {
    let emptyImageContainer = document.getElementById('empty-image-container');
    setImageInUseStatus(emptyImageContainer);

    let formData = new FormData();
    files.forEach(file => {
        formData.append('Images', file, file.name);
    });

    let response = await makeHttpRequest(`/api/images/entry-id/${entryId}`, POST, formData);
    if (!response.ok) {
        unsetImageInUseStatus(emptyImageContainer);

        console.error(response.status, response.statusText);
        return;
    }
        
    let uploadResults = await response.json();
    renderPreview(entryId, files, uploadResults);

    unsetImageInUseStatus(emptyImageContainer);
}


export function addDropAreaEvents(entryId, dropArea, fileInput, uploadButton) {
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
        await uploadImages(entryId, files);
    };

    uploadButton.onclick = () => fileInput.click();
}


export async function pasteImages(entryId) {
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
        
        await uploadImages(entryId, files);
    } catch (err) {
        console.error(err);
    }
}
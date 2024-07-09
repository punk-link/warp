export const eventNames = {
    uploadFinished: 'uploadFinished'
};


export const uploadFinishedEvent = new Event(eventNames.uploadFinished, {
    bubbles: true,
    cancelable: true,
    composed: false
});
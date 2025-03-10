export const eventNames = {
    uploadFinished: 'uploadFinished'
};


const EVENT_OPTIONS = Object.freeze({
    DEFAULT: {
        bubbles: true,
        cancelable: true,
        composed: false
    }
});


const handlers = {
    events: (() => {
        const createEvent = (name, options = EVENT_OPTIONS.DEFAULT) =>
            new Event(name, options);

        return {
            dispatch: {
                uploadFinished: (element = document) => {
                    const event = createEvent(eventNames.uploadFinished);
                    element.dispatchEvent(event);
                }
            }
        };
    })()
};


export const dispatchEvent = handlers.events.dispatch;
export const uploadFinishedEvent = new Event(eventNames.uploadFinished, EVENT_OPTIONS.DEFAULT);

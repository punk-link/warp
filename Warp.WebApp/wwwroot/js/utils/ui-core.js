export const dom = {
    get: id => document.getElementById(id),
    queryAll: selector => Array.from(document.querySelectorAll(selector)),
    query: selector => document.querySelector(selector)
};


export const uiState = {
    toggleClasses: (element, { add = [], remove = [] }) => {
        add.forEach(cls => element.classList.add(cls));
        remove.forEach(cls => element.classList.remove(cls));
        return element;
    },
    setElementValue: (element, value) => {
        element.value = value;
        return element;
    },
    setElementDisabled: (element, isDisabled) => {
        element.disabled = isDisabled;
        return element;
    }
};
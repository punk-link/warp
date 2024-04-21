function getRemSize () {
    let doc = document.documentElement;
    let fontSize = getComputedStyle(doc).fontSize;
    return parseFloat(fontSize);
}


export function remToPx(value) {
    return value * getRemSize();
}
function buildDisplayDigits(hours, minutes, seconds) {
    if (hours < 0 || minutes < 0 || seconds < 0) 
        return '<span class="text-light">00:00:00<span>';

    let timeString = `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
    let valuableIndex = getValuableIndex(timeString);

    let valuableText = timeString.substring(valuableIndex);
    valuableText = valuableText.replace(/:/g, '<span class="text-secondary">:</span>');

    return `<span class="text-light">${timeString.substring(0, valuableIndex)}</span>${valuableText}`;
}


function getValuableIndex(timeString) {
    for (let i = 0; i < timeString.length; i++) {
        if (timeString[i] !== '0' && timeString[i] !== ':') {
            return i;
        }
    }

    return 0;
}


function pad(timeSection) {
    return timeSection.toString().padStart(2, '0')
}


export function setCountdown(countdownElement, targetDate) {
    let updateCountdown = () => {
        let currentDate = new Date().getTime();
        let remainingTime = targetDate - currentDate;

        if (remainingTime <= 0) {
            countdownElement.innerHTML = buildDisplayDigits(0, 0, 0);
            clearInterval(countdownInterval);

            location.reload();
        }

        let seconds = Math.floor((remainingTime / 1000) % 60);
        let minutes = Math.floor((remainingTime / 1000 / 60) % 60);
        let hours = Math.floor((remainingTime / (1000 * 60 * 60)) % 24);

        countdownElement.innerHTML = buildDisplayDigits(hours, minutes, seconds);
    };

    let countdownInterval = setInterval(updateCountdown, 1000);
}
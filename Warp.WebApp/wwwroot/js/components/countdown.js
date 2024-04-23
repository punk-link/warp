 class Countdown {
    constructor(countdownElement, targetDate) {
        this.countdownElement = countdownElement;
        this.targetDate = targetDate;

        this.#setCountdown();
    }


    #getValuableIndex(timeString) {
        for (let i = 0; i < timeString.length; i++) {
            if (timeString[i] !== '0' && timeString[i] !== ':') {
                return i;
            }
        }

        return 0;
    }


    #buildDisplayDigits(hours, minutes, seconds) {
        if (hours < 0 || minutes < 0 || seconds < 0) 
            return '<span class="text-lighter">00:00:00<span>';

        let timeString = `${this.#pad(hours)}:${this.#pad(minutes)}:${this.#pad(seconds)}`;
        let valuableIndex = this.#getValuableIndex(timeString);

        let valuableText = timeString.substring(valuableIndex);
        valuableText = valuableText.replace(/:/g, '<span class="text-secondary">:</span>');

        return `<span class="text-lighter">${timeString.substring(0, valuableIndex)}</span>${valuableText}`;
    }


    #pad(timeSection) {
        return timeSection.toString().padStart(2, '0')
    }


    #setCountdown() {
        let updateCountdown = () => {
            let currentDate = new Date().getTime();
            let remainingTime = this.targetDate - currentDate;

            if (remainingTime <= 0) {
                this.countdownElement.innerHTML = this.#buildDisplayDigits(0, 0, 0);
                clearInterval(countdownInterval);

                location.reload();
            }

            let seconds = Math.floor((remainingTime / 1000) % 60);
            let minutes = Math.floor((remainingTime / 1000 / 60) % 60);
            let hours = Math.floor((remainingTime / (1000 * 60 * 60)) % 24);

            this.countdownElement.innerHTML = this.#buildDisplayDigits(hours, minutes, seconds);
        };

        let countdownInterval = setInterval(updateCountdown, 1000);
    }
}


export default Countdown;
import { dom, uiState } from '/js/utils/ui-core.js';

const EMPTY_COUNTDOWN = '<span class="text-gray-400">00:00:00<span>';
const MILLISECONDS = Object.freeze({
    SECOND: 1000,
    MINUTE: 1000 * 60,
    HOUR: 1000 * 60 * 60
});

const UPDATE_INTERVAL = 1000;

const elements = {
    getCountdown: () => dom.query('.countdown')
};


const handlers = {
    countdown: (() => {
        const formatTimeSection = timeSection => 
            timeSection.toString().padStart(2, '0');

        const findFirstSignificantDigitIndex = timeString => {
            const significantIndex = timeString
                .split('')
                .findIndex(char => char !== '0' && char !== ':');

            return significantIndex === -1 ? 0 : significantIndex;
        };

        const calculateTimeSegments = remainingTime => ({
            hours: Math.floor((remainingTime / MILLISECONDS.HOUR) % 24),
            minutes: Math.floor((remainingTime / MILLISECONDS.MINUTE) % 60),
            seconds: Math.floor((remainingTime / MILLISECONDS.SECOND) % 60)
        });

        const hasTimeRemaining = ({ hours, minutes, seconds }) => 
            hours > 0 || minutes > 0 || seconds > 0;

        const formatCountdown = (hours, minutes, seconds) => {
            if (!hasTimeRemaining({ hours, minutes, seconds })) 
                return EMPTY_COUNTDOWN;

            const timeString = [hours, minutes, seconds]
                .map(formatTimeSection)
                .join(':');

            const significantIndex = findFirstSignificantDigitIndex(timeString);
            if (significantIndex === 0) 
                return EMPTY_COUNTDOWN;

            const insignificantPart = timeString.substring(0, significantIndex);
            const significantPart = timeString
                .substring(significantIndex)
                .replace(/:/g, '<span class="text-secondary">:</span>');

            return `<span class="text-gray-400">${insignificantPart}</span>${significantPart}`;
        };

        const updateDisplay = (countdownElement, targetDate, interval) => {
            const currentTime = new Date().getTime();
            const remainingTime = targetDate - currentTime;

            if (remainingTime <= 0) {
                uiState.setElementHtml(countdownElement, EMPTY_COUNTDOWN);
                clearInterval(interval);
                location.reload();

                return;
            }

            const timeSegments = calculateTimeSegments(remainingTime);
            const formattedDisplay = formatCountdown(
                timeSegments.hours,
                timeSegments.minutes,
                timeSegments.seconds
            );

            uiState.setElementHtml(countdownElement, formattedDisplay);
        };

        return {
            init: (targetDate) => {
                const countdownElement = elements.getCountdown();
                if (!countdownElement) 
                    return;

                const interval = setInterval(
                    () => updateDisplay(countdownElement, targetDate, interval), 
                    UPDATE_INTERVAL
                );

                updateDisplay(countdownElement, targetDate, interval);
            }
        };
    })()
};


export const initializeCountdown = targetDate => {
    handlers.countdown.init(targetDate);
};
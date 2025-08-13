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


function buildCountdownMarkup(timeString, significantIndex) {
    const digitSpanClass = 'inline-block w-[1ch] text-center leading-none';
    const colonSpanClass = 'inline-block text-center leading-none';

    const chars = timeString.split('');
    const spans = chars.map((ch, i) => {
        if (ch === ':') {
            const color = i < significantIndex ? 'text-gray-300' : 'text-secondary';
            return `<span class="${colonSpanClass} ${color}">:</span>`;
        }

        const color = i < significantIndex ? 'text-gray-300' : '';
        return `<span class="${digitSpanClass} ${color}">${ch}</span>`;
    });

    return spans.join('');
}


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

        const getEmptyCountdownMarkup = () => buildCountdownMarkup('00:00:00', 0);

        const formatCountdown = (hours, minutes, seconds) => {
            if (!hasTimeRemaining({ hours, minutes, seconds })) 
                return getEmptyCountdownMarkup();

            const timeString = [hours, minutes, seconds]
                .map(formatTimeSection)
                .join(':');

            const significantIndex = findFirstSignificantDigitIndex(timeString);
            if (significantIndex === 0) 
                return getEmptyCountdownMarkup();

            return buildCountdownMarkup(timeString, significantIndex);
        };

        const updateDisplay = (countdownElement, targetDate) => {
            const currentTime = Date.now();
            const remainingTime = targetDate - currentTime;

            if (remainingTime <= 0) {
                uiState.setElementHtml(countdownElement, getEmptyCountdownMarkup());
                return false;
            }

            const timeSegments = calculateTimeSegments(remainingTime);
            const formattedDisplay = formatCountdown(timeSegments.hours, timeSegments.minutes, timeSegments.seconds);

            uiState.setElementHtml(countdownElement, formattedDisplay);

            return true;
        };

        const scheduleNextAlignedTick = (tick) => {
            const now = Date.now();
            const delay = MILLISECONDS.SECOND - (now % MILLISECONDS.SECOND);

            return setTimeout(tick, delay);
        };

        return {
            init: (targetDate) => {
                const countdownElement = elements.getCountdown();
                if (!countdownElement) 
                    return;

                countdownElement.classList.add('tabular-nums');

                let timeoutId = 0;

                const tick = () => {
                    requestAnimationFrame(() => {
                        const keepGoing = updateDisplay(countdownElement, targetDate);
                        if (!keepGoing) {
                            if (timeoutId)
                                clearTimeout(timeoutId);

                            window.location.reload();
                            return;
                        }

                        timeoutId = scheduleNextAlignedTick(tick);
                    });
                };

                requestAnimationFrame(() => {
                    updateDisplay(countdownElement, targetDate);
                    timeoutId = scheduleNextAlignedTick(tick);
                });

                setTimeout(() => {
                    countdownElement.classList.add('visible');
                }, 100);
            }
        };
    })()
};


export const initializeCountdown = targetDate => {
    handlers.countdown.init(targetDate);
};
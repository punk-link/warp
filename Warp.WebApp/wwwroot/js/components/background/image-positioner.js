import { dom, uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';


const CONFIG = Object.freeze({
    MIN_POSITION: 0,
    MAX_POSITION: 100
});


const handlers = {
    background: (() => {
        const generateRandomPosition = () => ({
            x: Math.floor(Math.random() * CONFIG.MAX_POSITION),
            y: Math.floor(Math.random() * CONFIG.MAX_POSITION)
        });

        const applyPosition = (element, position) => {
            element.style.top = `${position.y}vh`;
            element.style.left = `${position.x}vw`;
        };

        const showWithAnimation = (element) => {
            uiState.toggleClasses(element, {
                remove: [CSS_CLASSES.HIDDEN],
                add: [CSS_CLASSES.ANIMATE_SLOW]
            });
        };

        return {
            animateOnly: (element) => {
                if (!element)
                    return;

                showWithAnimation(element);
            },

            reposition: (element) => {
                if (!element) 
                    return;

                const position = generateRandomPosition();
                applyPosition(element, position);
                showWithAnimation(element);
            }
        };
    })()
};


export const animateBackgroundImage = (element) => {
    handlers.background.animateOnly(element);
};

export const repositionBackgroundImage = (element) => {
    handlers.background.reposition(element);
};
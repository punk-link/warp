function buildScalingInterval(lowerBound, upperBound, fontSize) {
    return {
        min: lowerBound,
        max: upperBound,
        size: fontSize + 'rem'
    }
}


function getContentLength(element) {
    if (element.value !== undefined)
        return element.value.length;

    return element.innerText.length;
}


function getScalingIntervals(config) {
    let initInterval = buildScalingInterval(0, config.lowerBound, config.maxFontSize);
        
    let results = [];
    results.push(initInterval);

    let currentValue = config.lowerBound;
    let currentSize = config.maxFontSize;
    for (let i = 0; i < config.numberOfSteps; i++) {
        let stepUpperBound = currentValue + config.step;
        if (i + 1 == config.numberOfSteps) 
            stepUpperBound = Number.MAX_SAFE_INTEGER;
            
        let interval = buildScalingInterval(currentValue, stepUpperBound, currentSize);
        results.push(interval);

        currentValue = currentValue + config.step;
        currentSize = currentSize - config.fontSizeStep;
    }

    return results;
}


export const FontScalerConfig = {
    lowerBound: 1000,
    step: 250,
    numberOfSteps: 6,
    maxFontSize: 1.25,
    fontSizeStep: 0.05
}


let defaultScalingIntervals = getScalingIntervals(FontScalerConfig);


export function applyFontScaling(element, config) {
    let scalingIntervals = defaultScalingIntervals;
    if (config !== undefined && config !== null)
        scalingIntervals = getScalingIntervals(FontScalerConfig);
    
    element.oninput = () => {
        let len = getContentLength(element);
            
        scalingIntervals.forEach(interval => {
            if (interval.min <= len && len < interval.max) {
                element.style.fontSize = interval.size;
                return;
            }
        });
    };
}

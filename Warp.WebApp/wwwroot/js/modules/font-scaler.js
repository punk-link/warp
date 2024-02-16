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


function getScalingIntervals(lowerBound, step, numberOfSteps, maxFontSize, fontSizeStep) {
    let initInterval = buildScalingInterval(0, lowerBound, maxFontSize);
        
    let results = [];
    results.push(initInterval);

    let currentValue = lowerBound;
    let currentSize = maxFontSize;
    for (let i = 0; i < numberOfSteps; i++) {
        let stepUpperBound = currentValue + step;
        if (i + 1 == numberOfSteps) 
            stepUpperBound = Number.MAX_SAFE_INTEGER;
            
        let interval = buildScalingInterval(currentValue, stepUpperBound, currentSize);
        results.push(interval);

        currentValue = currentValue + step;
        currentSize = currentSize - fontSizeStep;
    }

    return results;
}


export function applyFontScaling(element, lowerBound, step, numberOfSteps, maxFontSize, fontSizeStep) {
    let scalingIntervals = getScalingIntervals(lowerBound, step, numberOfSteps, maxFontSize, fontSizeStep);
    
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

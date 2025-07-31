import { Result } from './result.js';


export class UnitResult extends Result {
    constructor(isSuccess, details) {
        super(isSuccess, null, details);
    }


    /**
     * Creates a successful UnitResult.
     * @returns {UnitResult} A successful UnitResult instance.
     */
    static success() {
        return new UnitResult(true, null);
    }


    /**
     * Hides the value property for UnitResult.
     * @throws {Error} Always throws to prevent access.
     */
    get value() {
        throw new Error('UnitResult does not have a value.');
    }
}
export class Result {
    constructor(isSuccess, data, details) {
        this.#data = data;
        this.#details = details;
        this.#isSuccess = isSuccess;
    }


    static success(data) {
        return new Result(true, data, null);
    }


    static failure(details) {
        return new Result(false, null, details);
    }


    static fromJson(json) {
        if (json.detail) 
            return Result.failure(json.detail);

        return Result.success(json);
    }


    get error() {
        if (this.#isSuccess) 
            throw new Error('Cannot access error of a successful result');

        return this.#details;
    }


    get isSuccess() {
        return this.#isSuccess;
    }


    get isFailure() {
        return !this.#isSuccess;
    }


    get value() {
        if (!this.#isSuccess) 
            throw new Error('Cannot access value of a failed result');

        return this.#data;
    }


    #data
    #details;
    #isSuccess;
}
/** Error thrown when parsing ProblemDetails fails. */
export class ProblemDetailsParseError extends Error {
    raw: unknown
    constructor(message: string, raw: unknown) {
        super(message)
        this.name = 'ProblemDetailsParseError'
        this.raw = raw
    }
}
import { ProblemDetails } from "./problem-details/problem-details"
import { ApiResultKind } from "./enums/api-result-kind"


/** Represents a fatal API result. */
export interface ApiFatalResult {
    ok: false
    kind: ApiResultKind.Fatal
    status: number
    message: string
    problem?: ProblemDetails
    requestId?: string | null
}
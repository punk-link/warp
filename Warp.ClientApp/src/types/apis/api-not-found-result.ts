import { ProblemDetails } from "./problem-details/problem-details"
import { ApiResultKind } from "./enums/api-result-kind"


/** Represents a "not found" API result. */
export interface ApiNotFoundResult {
    ok: false
    kind: ApiResultKind.NotFound
    status: 404
    problem: ProblemDetails
}
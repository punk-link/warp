import { ProblemDetails } from "../problem-details"
import { ApiResultKind } from "./enums/api-result-kind"


/** Represents a validation API result. */
export interface ApiValidationResult {
    ok: false
    kind: ApiResultKind.Validation
    status: 400
    problem: ProblemDetails
    fieldErrors: Record<string, string[]>
}
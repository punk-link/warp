import { ProblemDetails } from "../problem-details"
import { ApiResultKind } from "./enums/api-result-kind"


export interface ApiNotFoundResult {
    ok: false
    kind: ApiResultKind.NotFound
    status: 404
    problem: ProblemDetails
}
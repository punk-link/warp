import { ProblemDetails } from "../problem-details"
import { ApiResultKind } from "./enums/api-result-kind"


export interface ApiFatalResult {
    ok: false
    kind: ApiResultKind.Fatal
    status: number
    message: string
    problem?: ProblemDetails
    requestId?: string | null
}
import { ApiResultKind } from "./enums/api-result-kind"


/** Represents a successful API result. */
export interface ApiSuccessResult<T> {
    ok: true
    kind: ApiResultKind.Success
    data: T
}
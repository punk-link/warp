import { ApiResultKind } from "./enums/api-result-kind"


export interface ApiSuccessResult<T> {
    ok: true
    kind: ApiResultKind.Success
    data: T
}
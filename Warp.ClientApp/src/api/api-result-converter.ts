import { fetchJson, ApiError } from './fetchHelper'
import type { ProblemDetails } from '../types/problem-details'
import { ApiResultKind } from '../types/apis/enums/api-result-kind'
import type { ApiResult } from '../types/apis/api-result'
import type { ApiSuccessResult } from '../types/apis/api-success-result'
import type { ApiValidationResult } from '../types/apis/api-validation-result'
import type { ApiNotFoundResult } from '../types/apis/api-not-found-result'
import type { ApiFatalResult } from '../types/apis/api-fatal-result'


function isValidationProblem(p?: ProblemDetails): p is ProblemDetails {
    return !!p && p.status === 400 && !!p.errors && Object.keys(p.errors).length > 0
}


function classify(p: ProblemDetails): ApiResultKind.Validation | ApiResultKind.NotFound | ApiResultKind.Fatal {
    if (p.status === 404) 
        return ApiResultKind.NotFound

    if (isValidationProblem(p)) 
        return ApiResultKind.Validation

    return ApiResultKind.Fatal
}


function fatal<T>(error: ApiError, problemDetails?: ProblemDetails): ApiResult<T> {
    if (problemDetails) {
        return {
            ok: false,
            kind: ApiResultKind.Fatal,
            status: problemDetails.status,
            problem: problemDetails,
            message: problemDetails.title || problemDetails.detail || error.message || 'Request failed',
            requestId: problemDetails.traceId || error.requestId
        }
    }

    return {
        ok: false,
        kind: ApiResultKind.Fatal,
        status: error.status ?? 0,
        message: error.message || 'Request failed',
        requestId: error.requestId
    }
}


function notFound<T>(problemDetails: ProblemDetails): ApiResult<T> {
    return { 
        ok: false, 
        kind: ApiResultKind.NotFound, 
        status: 404, 
        problem: problemDetails 
    }
}


function success<T>(data: T): ApiResult<T> {
    return { 
        ok: true, 
        kind: ApiResultKind.Success, 
        data 
    }
}


function validation<T>(problemDetails: ProblemDetails): ApiResult<T> {
    return { 
        ok: false, 
        kind: ApiResultKind.Validation, 
        status: 400, 
        problem: problemDetails, 
        fieldErrors: problemDetails.errors! 
    }
}


export async function fetchResult<T>(url: string, opts?: RequestInit): Promise<ApiResult<T>> {
    try {
        const data = await fetchJson<T>(url, opts)
        return success<T>(data)
    } catch (e) {
        const err = e as ApiError
        const problemDetails = err.problem
        if (!problemDetails) 
            return fatal<T>(err)

        const kind = classify(problemDetails)
        if (kind === ApiResultKind.Validation) 
            return validation<T>(problemDetails)

        if (kind === ApiResultKind.NotFound) 
            return notFound<T>(problemDetails)

        return fatal<T>(err, problemDetails)
    }
}


export function isSuccess<T>(r: ApiResult<T>): r is ApiSuccessResult<T> { 
    return r.ok && r.kind === ApiResultKind.Success 
}


export function isValidation<T>(r: ApiResult<T>): r is ApiValidationResult { 
    return !r.ok && r.kind === ApiResultKind.Validation
}


export function isNotFound<T>(r: ApiResult<T>): r is ApiNotFoundResult { 
    return !r.ok && r.kind === ApiResultKind.NotFound
}


export function isFatal<T>(r: ApiResult<T>): r is ApiFatalResult { 
    return !r.ok && r.kind === ApiResultKind.Fatal 
}

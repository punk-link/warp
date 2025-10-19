import { fetchJson, ApiError } from './fetchHelper'
import type { ProblemDetails } from '../types/problem-details'

export enum ApiResultKind {
    Success = 'success',
    Validation = 'validation',
    NotFound = 'notFound',
    Fatal = 'fatal'
}


export interface ApiValidationResult {
    ok: false
    kind: ApiResultKind.Validation
    status: 400
    problem: ProblemDetails
    fieldErrors: Record<string, string[]>
}


export interface ApiNotFoundResult {
    ok: false
    kind: ApiResultKind.NotFound
    status: 404
    problem: ProblemDetails
}


export interface ApiFatalResult {
    ok: false
    kind: ApiResultKind.Fatal
    status: number
    message: string
    problem?: ProblemDetails
    requestId?: string | null
}


export interface ApiSuccessResult<T> {
    ok: true
    kind: ApiResultKind.Success
    data: T
}


export type ApiResult<T> = ApiSuccessResult<T> | ApiValidationResult | ApiNotFoundResult | ApiFatalResult


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


export async function fetchResult<T>(url: string, opts?: RequestInit): Promise<ApiResult<T>> {
    try {
        const data = await fetchJson<T>(url, opts)
        return { 
            ok: true, 
            kind: ApiResultKind.Success, 
            data 
        }
    } catch (e) {
        const err = e as ApiError
        const problem = err.problem
        if (problem) {
            const kind = classify(problem)
            if (kind === ApiResultKind.Validation) {
                return { 
                    ok: false, 
                    kind, 
                    status: 400, 
                    problem: problem, 
                    fieldErrors: problem.errors! 
                }
            }

            if (kind === ApiResultKind.NotFound) {
                return { 
                    ok: false, 
                    kind, 
                    status: 404, 
                    problem: problem 
                }
            }

            return {
                ok: false,
                kind: ApiResultKind.Fatal,
                status: problem.status,
                problem: problem,
                message: problem.title || problem.detail || err.message || 'Request failed',
                requestId: problem.traceId || err.requestId
            }
        }

        return {
            ok: false,
            kind: ApiResultKind.Fatal,
            status: err.status ?? 0,
            message: err.message || 'Request failed',
            requestId: err.requestId
        }
    }
}


export function isSuccess<T>(r: ApiResult<T>): r is ApiSuccessResult<T> { return r.ok && r.kind === ApiResultKind.Success }
export function isValidation<T>(r: ApiResult<T>): r is ApiValidationResult { return !r.ok && r.kind === ApiResultKind.Validation }
export function isNotFound<T>(r: ApiResult<T>): r is ApiNotFoundResult { return !r.ok && r.kind === ApiResultKind.NotFound }
export function isFatal<T>(r: ApiResult<T>): r is ApiFatalResult { return !r.ok && r.kind === ApiResultKind.Fatal }

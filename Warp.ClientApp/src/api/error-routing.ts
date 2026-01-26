import type { ApiError } from '../types/apis/api-error'
import type { ApiResult } from '../types/apis/api-result';
import type { ApiFatalResult } from "../types/apis/api-fatal-result";
import type { ApiNotFoundResult } from "../types/apis/api-not-found-result";
import type { ApiValidationResult } from "../types/apis/api-validation-result";
import { ApiResultKind } from '../types/apis/enums/api-result-kind'
import { ViewNames } from '../router/enums/view-names';
import router from '../router'


/** Routes the specified API error if it requires routing. */
export function routeApiError(err: unknown) {
    const apiErr = err as Partial<ApiError>
    const status = typeof apiErr?.status === 'number' ? apiErr.status : undefined

    if (status == null)
        return

    if (status === 404) {
        router.replace({ name: ViewNames.NotFound })
        return
    }

    if (status >= 500 && status <= 599) {
        router.replace({ name: ViewNames.Error, query: buildErrorQuery(apiErr as ApiError) })
        return
    }

    // For all other statuses, do not redirect. Global error bridge will show in-place notification.
    return
}


/** Routes the specified API result if it represents an error that requires routing. */
export function routeApiResult<T>(result: ApiResult<T>): boolean {
    if (result.ok) 
        return false

    if ((result as ApiValidationResult).kind === ApiResultKind.Validation)
        return false
    
    const apiResult = result as ApiFatalResult | ApiNotFoundResult
    const query = buildResultQuery(apiResult)
    
    if (apiResult.status === 404) {
        router.replace({ name: ViewNames.NotFound })
        return true
    }

    if (apiResult.status >= 500 && apiResult.status <= 599) {
        router.replace({ name: ViewNames.Error, query })
        return true
    }

    return false
}


/** Wraps the specified function with error routing logic. */
export function withErrorRouting<TArgs extends any[], TResult>(fn: (...a: TArgs) => Promise<TResult>) {
    return async (...a: TArgs): Promise<TResult | undefined> => {
        try {
            return await fn(...a)
        } catch (e) {
            routeApiError(e)
            return undefined
        }
    }
}


function buildErrorParts(errors: Record<string, string[]>): string | undefined {
    const parts: string[] = []
    for (const [code, arr] of Object.entries(errors)) {
        for (const msg of arr)
            parts.push(`${encodeURIComponent(code)}:${encodeURIComponent(msg)}`)
    }
    return parts.length > 0 ? parts.join('|') : undefined
}


function buildErrorQuery(err: ApiError): Record<string, string> {
    const queryParams: Record<string, string> = {}
    const problem = err.problem
    if (problem) {
        queryParams.status = String(problem.status)
        if (problem.title) 
            queryParams.title = problem.title

        if (problem.detail) 
            queryParams.detail = problem.detail

        if (problem.traceId) 
            queryParams.rid = problem.traceId

        if (problem.errors && problem.status >= 500) {
            const parts: string[] = []
            for (const [code, arr] of Object.entries(problem.errors)) {
                for (const msg of arr as string[]) 
                    parts.push(`${encodeURIComponent(code)}:${encodeURIComponent(msg)}`)
            }

            if (parts.length) 
                queryParams.errs = parts.join('|')
        }
    } else {
        if (err.status) 
            queryParams.status = String(err.status)

        if (err.message) 
            queryParams.detail = err.message

        if (err.requestId) 
            queryParams.rid = err.requestId
    }

    return queryParams
}


function buildResultQuery(result: ApiFatalResult | ApiNotFoundResult): Record<string, string> {
    const query: Record<string, string> = {
        status: String(result.status)
    }
    
    if (result.problem) {
        if (result.problem.title) 
            query.title = result.problem.title
        
        if (result.problem.detail) 
            query.detail = result.problem.detail
        
        if (result.problem.traceId) 
            query.rid = result.problem.traceId
        
        if (result.kind === ApiResultKind.Fatal && result.problem.errors && result.problem.status >= 500) {
            const errorParts = buildErrorParts(result.problem.errors)
            if (errorParts)
                query.errs = errorParts
        }
    } else if (result.kind === ApiResultKind.Fatal && result.message) {
        query.detail = result.message
    }
    
    return query
}

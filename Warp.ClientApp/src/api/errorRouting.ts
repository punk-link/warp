import type { ApiError } from './fetchHelper'
import { type ApiResult, type ApiFatalResult, type ApiNotFoundResult, type ApiValidationResult, ApiResultKind } from './apiResult'
import router from '../router'


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
                for (const msg of arr) 
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


export function routeApiError(err: unknown) {
    const apiErr = err as Partial<ApiError>
    if (typeof apiErr?.status !== 'number') {
        // Unknown shape: just generic fallback
        router.replace({ name: 'Error' })
        return
    }

    router.replace({ 
        name: 'Error', 
        query: buildErrorQuery(apiErr as ApiError) 
    })
}


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

// Routes only fatal or notFound results; returns true if routed.
export function routeApiResult<T>(result: ApiResult<T>): boolean {
    if (result.ok) 
        return false

    if ((result as ApiValidationResult).kind === ApiResultKind.Validation)
        return false
    
    const apiResult = result as ApiFatalResult | ApiNotFoundResult
    const query: Record<string, string> = {
        status: String(apiResult.status)
    }
    
    if (apiResult.problem) {
        if (apiResult.problem.title) 
            query.title = apiResult.problem.title
        
        if (apiResult.problem.detail) 
            query.detail = apiResult.problem.detail
        
        if (apiResult.problem.traceId) 
            query.rid = apiResult.problem.traceId
        
        if (apiResult.kind === ApiResultKind.Fatal && apiResult.problem.errors && apiResult.problem.status >= 500) {
            const parts: string[] = []
            for (const [code, arr] of Object.entries(apiResult.problem.errors)) {
                for (const msg of arr) 
                    parts.push(`${encodeURIComponent(code)}:${encodeURIComponent(msg)}`)
            }

            if (parts.length) 
                query.errs = parts.join('|')
        }
    } else if (apiResult.kind === ApiResultKind.Fatal) {
        if (apiResult.message) 
            query.detail = apiResult.message
    }
    
    router.replace({ name: 'Error', query })
    return true
}

import type { ApiError } from '../types/apis/api-error'
import type { ApiFatalResult } from "../types/apis/api-fatal-result";
import type { ApiNotFoundResult } from "../types/apis/api-not-found-result";
import type { ApiValidationResult } from "../types/apis/api-validation-result";
import type { ApiResultKind } from '../types/apis/enums/api-result-kind'
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


export function routeApiError(err: unknown) {
    const apiErr = err as Partial<ApiError>
    const status = typeof apiErr?.status === 'number' ? apiErr.status : undefined

    if (status == null)
        return

    if (status === 404) {
        router.replace({ name: 'NotFound' })
        return
    }

    if (status >= 500 && status <= 599) {
        router.replace({ name: 'Error', query: buildErrorQuery(apiErr as ApiError) })
        return
    }

    // For all other statuses, do not redirect. Global error bridge will show in-place notification.
    return
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
    const status = apiResult.status
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
                for (const msg of arr as string[]) 
                    parts.push(`${encodeURIComponent(code)}:${encodeURIComponent(msg)}`)
            }

            if (parts.length) 
                query.errs = parts.join('|')
        }
    } else if (apiResult.kind === ApiResultKind.Fatal) {
        if (apiResult.message) 
            query.detail = apiResult.message
    }
    
    if (status === 404) {
        router.replace({ name: 'NotFound' })
        return true
    }

    if (status >= 500 && status <= 599) {
        router.replace({ name: 'Error', query })
        return true
    }

    return false
}

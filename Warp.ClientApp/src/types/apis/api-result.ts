import type { ApiFatalResult } from "./api-fatal-result";
import type { ApiNotFoundResult } from "./api-not-found-result";
import type { ApiSuccessResult } from "./api-success-result";
import type { ApiValidationResult } from "./api-validation-result";


/** Represents the result of an API call, which can be a success, validation error, not found error, or fatal error. */
export type ApiResult<T> = ApiSuccessResult<T> | ApiValidationResult | ApiNotFoundResult | ApiFatalResult
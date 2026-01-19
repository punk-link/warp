import type { ApiFatalResult } from "./api-fatal-result";
import type { ApiNotFoundResult } from "./api-not-found-result";
import type { ApiSuccessResult } from "./api-success-result";
import type { ApiValidationResult } from "./api-validation-result";


export type ApiResult<T> = ApiSuccessResult<T> | ApiValidationResult | ApiNotFoundResult | ApiFatalResult
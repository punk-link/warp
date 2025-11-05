/** Error handling mode for fetchJson.
 * Global: handled by the registered error bridge (notify/redirect).
 * Component: caller handles errors (inline), bridge is skipped.
 * Silent: no notify/redirect; suitable for background polling.
 */
export enum ErrorHandlingMode {
    Global = 0,
    Component = 1,
    Silent = 2
}

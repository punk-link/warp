function getAllowedExtensions(): Set<string> {
    const fromConfig = window.appConfig?.allowedImageExtensions
    if (fromConfig && fromConfig.length > 0)
        return new Set(fromConfig.map(e => e.toLowerCase()))

    return new Set()
}


/** * Checks if the given file has an allowed image extension based on the application configuration.
 * @param file The file to check.
 * @returns True if the file has an allowed image extension, false otherwise.
 */
export function isAllowedImageExtension(file: File): boolean {
    const dotIndex = file.name.lastIndexOf('.')
    if (dotIndex < 0)
        return false

    return getAllowedExtensions().has(file.name.slice(dotIndex).toLowerCase())
}

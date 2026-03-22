function getAllowedExtensions(): Set<string> {
    const fromConfig = window.appConfig?.allowedImageExtensions
    if (fromConfig && fromConfig.length > 0)
        return new Set(fromConfig.map(e => e.toLowerCase()))

    return new Set()
}


export function isAllowedImageExtension(file: File): boolean {
    const dotIndex = file.name.lastIndexOf('.')
    if (dotIndex < 0)
        return false

    return getAllowedExtensions().has(file.name.slice(dotIndex).toLowerCase())
}

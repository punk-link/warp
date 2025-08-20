export async function fetchJson<T = any>(url: string, opts: RequestInit = {}) {
  const response = await fetch(url, { credentials: 'include', headers: { 'Accept': 'application/json' }, ...opts })

  const contentType = response.headers.get('content-type') || ''
  if (!response.ok) {
    let body: any = null
    try {
      if (contentType.includes('application/json')) 
        body = await response.json()
      else 
        body = await response.text()
    } catch {}
    throw new Error(body?.message ?? `Request failed: ${response.status}`)
  }

  if (contentType.includes('application/json')) 
    return (await response.json()) as T
  
  return (await response.text()) as unknown as T
}

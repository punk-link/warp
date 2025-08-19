export async function fetchJson<T = any>(url: string, opts: RequestInit = {}) {
  const res = await fetch(url, { credentials: 'include', ...opts })
  const contentType = res.headers.get('content-type') || ''
  if (!res.ok) {
    let body: any = null
    try {
      if (contentType.includes('application/json')) 
        body = await res.json()
      else body = await res.text()
    } catch {}
    throw new Error(body?.message ?? `Request failed: ${res.status}`)
  }

  if (contentType.includes('application/json')) return (await res.json()) as T
  return (await res.text()) as unknown as T
}

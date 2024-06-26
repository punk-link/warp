const HttpMethod = { POST: 'POST', GET: 'GET', PUT: 'PUT', PATCH: 'PATCH', DELETE: 'DELETE' };
export const { POST, GET, PUT, PATCH, DELETE } = HttpMethod;

export async function makeHttpRequest(URL, method, body = null) {
    let responce = await fetch(URL, {
        method: method,
        body: body != null ? JSON.stringify(body) : null,
        headers: body != null ? headers : null
    });

    return responce;
}

const headers = {
    'Accept': 'application/json; charset=utf-8',
    'Content-Type': 'application/json; charset=utf-8'
}
﻿const HttpMethod = { POST: 'POST', GET: 'GET', PUT: 'PUT', PATCH: 'PATCH', DELETE: 'DELETE' };
export const { POST, GET, PUT, PATCH, DELETE } = HttpMethod;


const headers = {
    'Accept': 'application/json; charset=utf-8',
    'Content-Type': 'application/json; charset=utf-8'
}


export async function makeHttpRequest(URL, method, body = null) {
    let response = await fetch(URL, {
        method: method,
        body: body
    });

    return response;
}
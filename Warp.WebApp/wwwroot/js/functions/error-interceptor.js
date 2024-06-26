export async function navigateToErrorPage(problemDetails) {
    await fetch('/error', {
        method: 'POST',
        body: JSON.stringify({ problemDetails: problemDetails }),
        headers: {
            'Accept': 'application/json; charset=utf-8',
            'Content-Type': 'application/json; charset=utf-8'
        },
    });
}
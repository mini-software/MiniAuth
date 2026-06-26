
const form = document.getElementById('login-form');
const message = document.getElementById('message');
const loginButton = document.getElementById('login-button');
const rootPath = getRootPath();

form.addEventListener('submit', async event => {
    event.preventDefault();
    setMessage('');
    loginButton.disabled = true;

    const username = document.getElementById('username').value.trim();
    const password = document.getElementById('password').value;
    const remember = document.getElementById('remember').checked;

    try {
        const response = await fetch(`${rootPath}login`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ username, password, remember })
        });

        const payload = await readJson(response);
        if (!response.ok || payload.ok === false || payload.code >= 400) {
            throw new Error(payload.message || 'Login failed.');
        }

        const data = payload.data || payload;
        if (data.accessToken) {
            localStorage.setItem('X-MiniAuth-Token', data.accessToken);
        }

        window.location.href = getReturnUrl();
    } catch (error) {
        setMessage(error.message || 'Login failed.');
    } finally {
        loginButton.disabled = false;
    }
});

function getRootPath() {
    const path = window.location.pathname;
    const lastSlash = path.lastIndexOf('/');
    return lastSlash >= 0 ? path.slice(0, lastSlash + 1) : '/';
}

function getReturnUrl() {
    const url = new URL(window.location.href);
    return url.searchParams.get('returnUrl')
        || url.searchParams.get('ReturnUrl')
        || `${rootPath}index.html`;
}

async function readJson(response) {
    const text = await response.text();
    if (!text) {
        return {};
    }

    try {
        return JSON.parse(text);
    } catch {
        return { message: text };
    }
}

function setMessage(value) {
    message.textContent = value;
}
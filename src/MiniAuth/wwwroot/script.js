
window.onload = function () {
    var userLang = navigator.language || navigator.userLanguage;
    var translations = {
        'fr': {
            username: 'Nom d\'utilisateur',
            password: 'Mot de passe',
            login: 'S\'identifier'
        },
        'zh-CN': {
            username: '用户名',
            password: '密码',
            login: '登录'
        },
        'zh-TW': {
            username: '帳號',
            password: '密碼',
            login: '登入'
        },
        'zh-HK': {
            username: '帳號',
            password: '密碼',
            login: '登入'
        },
        'ko': {
            username: '사용자 이름',
            password: '암호',
            login: '로그인'
        },
        'ja': {
            username: 'ユーザー名',
            password: 'パスワード',
            login: 'ログイン'
        },
        'de': {
            username: 'Benutzername',
            password: 'Passwort',
            login: 'Anmeldung'
        },
        'it': {
            username: 'Nome utente',
            password: 'Parola d\'ordine',
            login: 'Accesso'
        },
        'pt': {
            username: 'Nome de usuário',
            password: 'Senha',
            login: 'Entrar'
        },
        'ru': {
            username: 'Имя пользователя',
            password: 'Пароль',
            login: 'Авторизоваться'
        },
        'es': {
            username: 'Nombre de usuario',
            password: 'Contraseña',
            login: 'Iniciar sesión'
        },
        'default': {
            username: 'Username',
            password: 'Password',
            login: 'Login'
        }
    };

    var langSpecific = translations[userLang] || translations['default'];

    var loginForm = document.getElementById('loginForm');
    var title = document.getElementById('title');
    var usernameInput = document.getElementById('username');
    var passwordInput = document.getElementById('password');
    var loginButton = loginForm.querySelector('button');

    usernameInput.placeholder = langSpecific.username;
    passwordInput.placeholder = langSpecific.password;
    loginButton.textContent = langSpecific.login;
    title.textContent = langSpecific.login;
};

document.getElementById('loginForm').addEventListener('submit', function (event) {
    event.preventDefault(); 

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    // get url returnUrl from query string
    const url = new URL(window.location.href);
    const returnUrl = url.searchParams.get('returnUrl') || '/';

    const xhr = new XMLHttpRequest();
    xhr.open('POST', 'login');
    xhr.setRequestHeader('Content-Type', 'application/json');

    xhr.onload = function () {
        if (xhr.status === 200) { 
            const token = xhr.getResponseHeader('X-MiniAuth-Token') ;
            if (token) { 
                localStorage.setItem('token', token); 
                window.location.href = returnUrl; 
            } else {  
                document.getElementById('message').textContent = 'Login successful but token not found.';
            }
        } else {  
            document.getElementById('message').textContent = 'Login failed. Please check your credentials.'; 
        }
    };

    xhr.onerror = function () { 
        document.getElementById('message').textContent = 'An error occurred while trying to log in.'; // You might want to change this message to something more user-friendly  
    };
    
    xhr.send(JSON.stringify({ username: username, password: password }));
});
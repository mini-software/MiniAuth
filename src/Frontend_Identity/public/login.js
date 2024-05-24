
window.onload = function () {
    var userLang = navigator.language || navigator.userLanguage;
    var translations = {
        'fr': {
            username: 'Nom d\'utilisateur',
            password: 'Mot de passe',
            login: 'S\'identifier',
            remember: 'Se souvenir de moi'
        },
        'zh-CN': {
            username: '用户名',
            password: '密码',
            login: '登录',
            remember: '记住我'
        },
        'zh-TW': {
            username: '帳號',
            password: '密碼',
            login: '登入',
            remember: '記住我'
        },
        'zh-HK': {
            username: '帳號',
            password: '密碼',
            login: '登入',
            remember: '記住我'
        },
        'ko': {
            username: '사용자 이름',
            password: '암호',
            login: '로그인',
            remember: '자동 로그인'
        },
        'ja': {
            username: 'ユーザー名',
            password: 'パスワード',
            login: 'ログイン',
            remember: 'ログインを保持'
        },
        'de': {
            username: 'Benutzername',
            password: 'Passwort',
            login: 'Anmeldung',
            remember: 'Anmeldung bleiben'
        },
        'it': {
            username: 'Nome utente',
            password: 'Parola d\'ordine',
            login: 'Accesso',
            remember: 'Ricordami'
        },
        'pt': {
            username: 'Nome de usuário',
            password: 'Senha',
            login: 'Entrar',
            remember: 'Lembre-se de mim'
        },
        'ru': {
            username: 'Имя пользователя',
            password: 'Пароль',
            login: 'Авторизоваться',
            remember: 'Запомнить меня'
        },
        'es': {
            username: 'Nombre de usuario',
            password: 'Contraseña',
            login: 'Iniciar sesión',
            remember: 'Recordarme'
        },
        'default': {
            username: 'Username',
            password: 'Password',
            login: 'Login',
            remember: 'Remember me'
        }
    };

    var langSpecific = translations[userLang] || translations['default'];

    var loginForm = document.getElementById('loginForm');
    var title = document.getElementById('title');
    var usernameInput = document.getElementById('username');
    var passwordInput = document.getElementById('password');
    var loginButton = loginForm.querySelector('button');
    var rememberCheckbox = document.getElementById('rememberCheckbox');

    usernameInput.placeholder = langSpecific.username;
    passwordInput.placeholder = langSpecific.password;
    loginButton.textContent = langSpecific.login;
    title.textContent = langSpecific.login;
    rememberCheckbox.textContent = langSpecific.remember;
};
document.getElementById('loginForm').addEventListener('submit', function (event) {
    event.preventDefault(); 

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const remember = document.getElementById('remember').checked;
    const url = new URL(window.location.href);
    const returnUrl = url.searchParams.get('returnUrl') || '/';

    const xhr = new XMLHttpRequest();
    xhr.open('POST', 'http://localhost:5566/MiniAuth/login');
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.withCredentials = true;


    xhr.onload = function () {
        if (xhr.status === 200) { 
            const token = JSON.parse(xhr.responseText)['X-MiniAuth-Token'];
            if (token!=undefined && token!=null )  
                localStorage.setItem('X-MiniAuth-Token', token); 

            window.location.href = returnUrl; 
        } else {  
            document.getElementById('message').textContent = 'Login failed. Please check your credentials.'; 
        }
    };

    xhr.onerror = function () { 
        document.getElementById('message').textContent = 'An error occurred while trying to log in.'; // You might want to change this message to something more user-friendly  
    };
    
    xhr.send(JSON.stringify({ username: username, password: password,remember:remember }));
});
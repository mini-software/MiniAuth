document.getElementById('loginForm').addEventListener('submit', function (event) {
    event.preventDefault(); 

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const returnUrl = document.getElementById('returnUrl').value || '/';

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
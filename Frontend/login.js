// Configuración de la API
const API_BASE_URL = 'http://localhost:5081/api';
let authToken = null;
let isRedirecting = false;

document.addEventListener('DOMContentLoaded', function () {
    console.log('Login script loaded');

    // Revisar si ya hay un token
    authToken = sessionStorage.getItem('authToken');
    if (authToken) {
        console.log('Usuario ya autenticado, redirigiendo...');
        window.location.replace('./dashboard.html');
        return;
    }

    // Evento del formulario
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
        console.log('Login form listener attached');
    }
});

// Manejar login
async function handleLogin(event) {
    event.preventDefault();

    if (isRedirecting) return;

    const formData = new FormData(event.target);
    const loginData = {
        username: formData.get('username'),
        password: formData.get('password')
    };

    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(loginData)
        });

        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

        const result = await response.json();
        authToken = result.token || result.access_token;

        if (!authToken) throw new Error('No se recibió token');

        sessionStorage.setItem('authToken', authToken);
        sessionStorage.setItem('userData', JSON.stringify(result.user || result));

        alert('Inicio de sesión exitoso ✅');
        isRedirecting = true;
        window.location.replace('./dashboard.html');
    } catch (error) {
        console.error('Error login:', error);
        alert('Credenciales inválidas ❌');
    }
}

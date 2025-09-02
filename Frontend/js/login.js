// Configuración de la API
// API_BASE_URL se carga desde config.js
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
        userName: formData.get('username'),
        password: formData.get('password')
    };

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(loginData)
        });

        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

        const result = await response.json();
        authToken = result.token || result.access_token;

        if (!authToken) throw new Error('No se recibió token');

        sessionStorage.setItem('authToken', authToken);
        
        // Guardar toda la información del usuario incluyendo roles y permisos
        const userData = {
            userId: result.userId,
            userName: result.userName,
            firstName: result.firstName,
            lastName: result.lastName,
            email: result.email,
            roles: result.roles || [],
            permissions: result.permissions || []
        };
        
        sessionStorage.setItem('userData', JSON.stringify(userData));
        
        console.log('Login successful for user:', userData.userName);
        console.log('User roles:', userData.roles);
        console.log('User permissions:', userData.permissions);

        showToast('Inicio de sesión exitoso', 'success');
        isRedirecting = true;
        setTimeout(() => {
            window.location.replace('./dashboard.html');
        }, 1000);
    } catch (error) {
        console.error('Error login:', error);
        showToast('Credenciales inválidas. Intenta nuevamente', 'error');
    }
}

// Configuración de la API
const API_BASE_URL = 'http://localhost:5000/api';
let isRedirecting = false;

document.addEventListener('DOMContentLoaded', function () {
    console.log('Register script loaded');

    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
        console.log('Register form listener attached');
    }
});

// Manejar registro
async function handleRegister(event) {
    event.preventDefault();

    if (isRedirecting) return;

    const formData = new FormData(event.target);

    // Validar contraseñas
    const password = formData.get('password');
    const confirmPassword = formData.get('confirmPassword');
    if (password !== confirmPassword) {
        showToast('Las contraseñas no coinciden', 'error');
        return;
    }

    // Formato correcto según la API RegisterDto
    const registerData = {
        firstName: formData.get('firstName'),
        lastName: formData.get('lastName'),
        email: formData.get('email'),
        userName: formData.get('username'),
        password: password
    };

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(registerData)
        });

        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

        await response.json();
        showToast('Registro exitoso. Ahora puedes iniciar sesión', 'success');
        isRedirecting = true;
        setTimeout(() => {
            window.location.replace('./index.html');
        }, 2000);
    } catch (error) {
        console.error('Error registro:', error);
        showToast('Error en el registro. Intenta nuevamente', 'error');
    }
}

// Configuración de la API
const API_BASE_URL = 'http://localhost:5081/api';
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
        alert('Las contraseñas no coinciden ❌');
        return;
    }

    const userData = {
        username: formData.get('username'),
        email: formData.get('email'),
        password,
        active: true
    };

    const personData = {
        firstName: formData.get('firstName'),
        lastName: formData.get('lastName'),
        email: formData.get('email'),
        phone: formData.get('phone') || null,
        address: formData.get('address') || null,
        birthDate: formData.get('birthDate') || null,
        gender: formData.get('gender') || null,
        active: true
    };

    const registerData = { user: userData, person: personData };

    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(registerData)
        });

        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

        await response.json();
        alert('Registro exitoso 🎉 Ahora puedes iniciar sesión');
        isRedirecting = true;
        window.location.replace('./login.html');
    } catch (error) {
        console.error('Error registro:', error);
        alert('Error en el registro ❌');
    }
}

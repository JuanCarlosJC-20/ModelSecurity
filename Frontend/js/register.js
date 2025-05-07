
const apiModelSecurity = 'http://localhost:5081';

const registration= document.getElementById('registration');
const firstName = document.getElementById('firstname');
const lastName = document.getElementById('lastname');
const email = document.getElementById('email');
const password = document.getElementById('password');
const confirmPassword = document.getElementById('confirm-password');

registration.addEventListener('submit', async (e) => {
    e.preventDefault();

    if (!registration.checkValidity()) {
        alert("Llena todos los campos");
        return;
    }
    if (password.value !== confirmPassword.value) {
        alert("Las contraseñas no coinciden");
        return;
    }
    const registerData = {
        firstName: firstName.value,
        lastName: lastName.value,
        email: email.value,
        userName: firstName.value.toLowerCase() + lastName.value.toLowerCase(),
        password: password.value
    };
    try {
        const response = await fetch(`${apiModelSecurity}/api/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(registerData)
        });
        if (!response.ok) {
            alert('Error al registrar usuario');
            return;
        }
        
        const resultado = await response.json();
        console.log('Registro exitoso:', resultado);
        registration.reset(); // Limpiar el formulario después del registro
        alert('Te registraste');

    } catch (err) {
        console.error('Error en el registro:', err);
        alert('Ocurrió un error al registrarse.');
    }
});

   

  


    
// Variables globales
let currentSection = 'dashboard';

// Navegación entre secciones
function showSection(sectionId) {
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });
    document.getElementById(sectionId).classList.add('active');

    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });
    document.querySelector(`.nav-link[data-section="${sectionId}"]`).classList.add('active');

    currentSection = sectionId;

user
  
}


// Configurar navegación al cargar
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (event) => {
            event.preventDefault();
            const sectionId = link.getAttribute('data-section');
            showSection(sectionId);
        });
    });

    // Mostrar sección inicial
    showSection(currentSection);
});

/// script.js

async function crearUsuario(usuario) {
    try {
        const response = await fetch('http://localhost:5081/api/user', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(usuario)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Error HTTP ${response.status}: ${errorText}`);
        }

        const data = await response.json();
        console.log('Usuario creado:', data);
        alert('Usuario creado exitosamente');
        return data;
    } catch (error) {
        console.error('Error en fetch POST:', error);
        alert('Error al crear usuario: ' + error.message);
    }
}

document.getElementById('formularioUsuario').addEventListener('submit', function (event) {
    event.preventDefault();

    const usuario = {
        userName: document.getElementById('userName').value,
        code: document.getElementById('code').value,
        personId: parseInt(document.getElementById('personId').value),
        active: true // Puedes omitir si tu backend usa DEFAULT 1
    };

    crearUsuario(usuario);
});

// Función para contar los usuarios y actualizar el contador
async function contarUsuarios() {
    try {
        const response = await fetch('http://localhost:5081/api/user'); // Asegúrate de que esta URL sea correcta
        if (!response.ok) {
            throw new Error(`Error HTTP: ${response.status}`);
        }
        
        const usuarios = await response.json(); // Suponiendo que tu API devuelve un arreglo de usuarios
        const contador = usuarios.length; // Cuenta la cantidad de usuarios
        document.getElementById('user-count').textContent = contador; // Actualiza el contenido del elemento
    } catch (error) {
        console.error('Error al contar usuarios:', error);
    }
}

// Llamar a la función para contar los usuarios al cargar la página
window.onload = contarUsuarios;


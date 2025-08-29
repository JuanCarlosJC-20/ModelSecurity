// Configuración de la API
const API_BASE_URL = 'http://localhost:5081/api';
let authToken = null;
let dashboardInitialized = false;

// Verificar autenticación al cargar la página
document.addEventListener('DOMContentLoaded', function() {
    console.log('Dashboard loaded');
    
    // Evitar múltiples inicializaciones
    if (dashboardInitialized) {
        console.log('Dashboard already initialized');
        return;
    }
    
    // Verificar si hay token
    authToken = sessionStorage.getItem('authToken');
    
    if (!authToken) {
        console.log('No auth token found, redirecting to login');
        window.location.replace('./login.html');
        return;
    }

    console.log('Valid token found, initializing dashboard');
    dashboardInitialized = true;
    
    // Inicializar dashboard
    initializeDashboard();
    loadDashboardData();
    setupNavigation();
});

// Inicializar dashboard
function initializeDashboard() {
    console.log('Initializing dashboard...');
    
    // Mostrar información del usuario
    const userData = JSON.parse(sessionStorage.getItem('userData') || '{}');
    const userName = userData.username || userData.user?.username || 'Usuario';
    
    const userNameElement = document.getElementById('userName');
    if (userNameElement) {
        userNameElement.textContent = userName;
    }
    
    console.log('Dashboard initialized for user:', userName);
}

// Cargar datos del dashboard
async function loadDashboardData() {
    console.log('Loading dashboard data...');
    
    try {
        showLoading(true);
        
        // Cargar estadísticas
        await loadStats();
        
        // Cargar datos de las tablas
        await loadUsers();
        await loadPersons();
        await loadRoles();
        await loadForms();
        
        console.log('Dashboard data loaded successfully');
    } catch (error) {
        console.error('Error loading dashboard data:', error);
        showMessage('Error al cargar los datos del dashboard', 'error');
    } finally {
        showLoading(false);
    }
}

// Cargar estadísticas
async function loadStats() {
    try {
        console.log('Loading stats...');
        
        // Por ahora usar datos simulados - reemplaza con llamadas reales a la API
        const stats = {
            users: 0,
            persons: 0,
            roles: 0,
            forms: 0
        };
        
        // Actualizar elementos del DOM si existen
        const totalUsersElement = document.getElementById('totalUsers');
        const totalPersonsElement = document.getElementById('totalPersons');
        const totalRolesElement = document.getElementById('totalRoles');
        const totalFormsElement = document.getElementById('totalForms');
        
        if (totalUsersElement) totalUsersElement.textContent = stats.users;
        if (totalPersonsElement) totalPersonsElement.textContent = stats.persons;
        if (totalRolesElement) totalRolesElement.textContent = stats.roles;
        if (totalFormsElement) totalFormsElement.textContent = stats.forms;
        
        console.log('Stats loaded:', stats);
        
    } catch (error) {
        console.error('Error loading stats:', error);
    }
}

// Cargar usuarios
async function loadUsers() {
    try {
        console.log('Loading users...');
        const tableBody = document.querySelector('#usersTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #6b7280;">No hay datos disponibles</td></tr>';
        }
    } catch (error) {
        console.error('Error loading users:', error);
    }
}

// Cargar personas
async function loadPersons() {
    try {
        console.log('Loading persons...');
        const tableBody = document.querySelector('#personsTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="6" style="text-align: center; padding: 2rem; color: #6b7280;">No hay datos disponibles</td></tr>';
        }
    } catch (error) {
        console.error('Error loading persons:', error);
    }
}

// Cargar roles
async function loadRoles() {
    try {
        console.log('Loading roles...');
        const tableBody = document.querySelector('#rolesTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #6b7280;">No hay datos disponibles</td></tr>';
        }
    } catch (error) {
        console.error('Error loading roles:', error);
    }
}

// Cargar formularios
async function loadForms() {
    try {
        console.log('Loading forms...');
        const tableBody = document.querySelector('#formsTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #6b7280;">No hay datos disponibles</td></tr>';
        }
    } catch (error) {
        console.error('Error loading forms:', error);
    }
}

// Configurar navegación
function setupNavigation() {
    const navLinks = document.querySelectorAll('.nav-link');
    const sections = document.querySelectorAll('.content-section');
    
    if (navLinks.length === 0 || sections.length === 0) {
        console.warn('Navigation elements not found');
        return;
    }
    
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetSection = this.getAttribute('data-section');
            console.log('Navigating to section:', targetSection);
            
            // Remover clase active de todos los links y secciones
            navLinks.forEach(l => l.classList.remove('active'));
            sections.forEach(s => s.classList.remove('active'));
            
            // Activar el link clickeado y su sección correspondiente
            this.classList.add('active');
            const targetSectionElement = document.getElementById(`${targetSection}-section`);
            if (targetSectionElement) {
                targetSectionElement.classList.add('active');
            }
            
            // Actualizar título
            const titles = {
                'overview': 'Vista General',
                'users': 'Gestión de Usuarios',
                'persons': 'Gestión de Personas',
                'roles': 'Gestión de Roles',
                'permissions': 'Gestión de Permisos',
                'forms': 'Gestión de Formularios',
                'modules': 'Gestión de Módulos'
            };
            
            const pageTitle = document.getElementById('pageTitle');
            if (pageTitle) {
                pageTitle.textContent = titles[targetSection] || 'Dashboard';
            }
        });
    });
    
    console.log('Navigation setup completed');
}

// Función para mostrar loading
function showLoading(show = true) {
    const spinner = document.getElementById('loadingSpinner');
    if (spinner) {
        spinner.classList.toggle('active', show);
    }
}

// Función para mostrar mensajes
function showMessage(message, type = 'info') {
    // Crear elemento de mensaje si no existe
    let messageDiv = document.getElementById('message');
    if (!messageDiv) {
        messageDiv = document.createElement('div');
        messageDiv.id = 'message';
        messageDiv.className = 'message';
        document.body.appendChild(messageDiv);
    }
    
    messageDiv.textContent = message;
    messageDiv.className = `message ${type}`;
    messageDiv.style.display = 'block';
    messageDiv.style.position = 'fixed';
    messageDiv.style.top = '20px';
    messageDiv.style.right = '20px';
    messageDiv.style.zIndex = '10000';

    // Ocultar mensaje después de 5 segundos
    setTimeout(() => {
        messageDiv.style.display = 'none';
    }, 5000);
}

// Función para hacer peticiones autenticadas
async function authenticatedFetch(url, options = {}) {
    const token = sessionStorage.getItem('authToken');
    
    if (!token) {
        console.log('No token available for authenticated request');
        window.location.replace('./login.html');
        return null;
    }

    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    };

    const mergedOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...options.headers
        }
    };

    try {
        const response = await fetch(url, mergedOptions);
        
        if (response.status === 401) {
            console.log('Token expired, redirecting to login');
            sessionStorage.removeItem('authToken');
            sessionStorage.removeItem('userData');
            window.location.replace('./login.html');
            return null;
        }

        return response;
    } catch (error) {
        console.error('Error en petición autenticada:', error);
        throw error;
    }
}

// Función para cerrar sesión
function logout() {
    console.log('Logging out from dashboard...');
    sessionStorage.removeItem('authToken');
    sessionStorage.removeItem('userData');
    dashboardInitialized = false;
    window.location.replace('./login.html');
}

// Funciones para modales (placeholder)
function showModal(modalId) {
    console.log('Opening modal:', modalId);
    showMessage(`Modal ${modalId} no implementado aún`, 'info');
}

// Exportar funciones para uso global
window.logout = logout;
window.authenticatedFetch = authenticatedFetch;
window.showModal = showModal;
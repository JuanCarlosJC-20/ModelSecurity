// Configuración de la API
// API_BASE_URL se carga desde config.js
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
        window.location.replace('./index.html');
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
        await loadPermissions();
        await loadModules();
        
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
        
        // Cargar datos reales desde la API
        const [usersResponse, personsResponse, rolesResponse, formsResponse] = await Promise.all([
            authenticatedFetch(`${API_BASE_URL}/User`),
            authenticatedFetch(`${API_BASE_URL}/Person`),
            authenticatedFetch(`${API_BASE_URL}/Rol`),
            authenticatedFetch(`${API_BASE_URL}/Form`)
        ]);
        
        let stats = { users: 0, persons: 0, roles: 0, forms: 0 };
        
        if (usersResponse && usersResponse.ok) {
            const users = await usersResponse.json();
            stats.users = users.length;
        }
        
        if (personsResponse && personsResponse.ok) {
            const persons = await personsResponse.json();
            stats.persons = persons.length;
        }
        
        if (rolesResponse && rolesResponse.ok) {
            const roles = await rolesResponse.json();
            stats.roles = roles.length;
        }
        
        if (formsResponse && formsResponse.ok) {
            const forms = await formsResponse.json();
            stats.forms = forms.length;
        }
        
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
        const response = await authenticatedFetch(`${API_BASE_URL}/User`);
        const tableBody = document.querySelector('#usersTable tbody');
        
        if (!tableBody) return;
        
        if (!response || !response.ok) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar usuarios</td></tr>';
            return;
        }
        
        const users = await response.json();
        
        if (users.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #6b7280;">No hay usuarios registrados</td></tr>';
            return;
        }
        
        tableBody.innerHTML = users.map(user => `
            <tr>
                <td>${user.id}</td>
                <td>${user.userName}</td>
                <td>${user.code || 'N/A'}</td>
                <td><span class="status-badge ${user.active ? 'active' : 'inactive'}">${user.active ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn-icon" onclick="editUser(${user.id})" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon danger" onclick="deleteUser(${user.id})" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');
        
        console.log('Users loaded:', users.length);
        
    } catch (error) {
        console.error('Error loading users:', error);
        const tableBody = document.querySelector('#usersTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar usuarios</td></tr>';
        }
    }
}

// Cargar personas
async function loadPersons() {
    try {
        console.log('Loading persons...');
        const response = await authenticatedFetch(`${API_BASE_URL}/Person`);
        const tableBody = document.querySelector('#personsTable tbody');
        
        if (!tableBody) return;
        
        if (!response || !response.ok) {
            tableBody.innerHTML = '<tr><td colspan="6" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar personas</td></tr>';
            return;
        }
        
        const persons = await response.json();
        
        if (persons.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="6" style="text-align: center; padding: 2rem; color: #6b7280;">No hay personas registradas</td></tr>';
            return;
        }
        
        tableBody.innerHTML = persons.map(person => `
            <tr>
                <td>${person.id}</td>
                <td>${person.firstName}</td>
                <td>${person.lastName}</td>
                <td>${person.email}</td>
                <td>N/A</td>
                <td>
                    <button class="btn-icon" onclick="editPerson(${person.id})" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon danger" onclick="deletePerson(${person.id})" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');
        
        console.log('Persons loaded:', persons.length);
        
    } catch (error) {
        console.error('Error loading persons:', error);
        const tableBody = document.querySelector('#personsTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="6" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar personas</td></tr>';
        }
    }
}

// Cargar roles
async function loadRoles() {
    try {
        console.log('Loading roles...');
        const response = await authenticatedFetch(`${API_BASE_URL}/Rol`);
        const tableBody = document.querySelector('#rolesTable tbody');
        
        if (!tableBody) return;
        
        if (!response || !response.ok) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar roles</td></tr>';
            return;
        }
        
        const roles = await response.json();
        
        if (roles.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #6b7280;">No hay roles registrados</td></tr>';
            return;
        }
        
        tableBody.innerHTML = roles.map(role => `
            <tr>
                <td>${role.id}</td>
                <td>${role.name}</td>
                <td>${role.description || 'N/A'}</td>
                <td><span class="status-badge ${role.active ? 'active' : 'inactive'}">${role.active ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn-icon" onclick="editRole(${role.id})" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon danger" onclick="deleteRole(${role.id})" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');
        
        console.log('Roles loaded:', roles.length);
        
    } catch (error) {
        console.error('Error loading roles:', error);
        const tableBody = document.querySelector('#rolesTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar roles</td></tr>';
        }
    }
}

// Cargar formularios
async function loadForms() {
    try {
        console.log('Loading forms...');
        const response = await authenticatedFetch(`${API_BASE_URL}/Form`);
        const tableBody = document.querySelector('#formsTable tbody');
        
        if (!tableBody) return;
        
        if (!response || !response.ok) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar formularios</td></tr>';
            return;
        }
        
        const forms = await response.json();
        
        if (forms.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #6b7280;">No hay formularios registrados</td></tr>';
            return;
        }
        
        tableBody.innerHTML = forms.map(form => `
            <tr>
                <td>${form.id}</td>
                <td>${form.name}</td>
                <td>${form.code || 'N/A'}</td>
                <td><span class="status-badge ${form.active ? 'active' : 'inactive'}">${form.active ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn-icon" onclick="editForm(${form.id})" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon danger" onclick="deleteForm(${form.id})" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');
        
        console.log('Forms loaded:', forms.length);
        
    } catch (error) {
        console.error('Error loading forms:', error);
        const tableBody = document.querySelector('#formsTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 2rem; color: #dc2626;">Error al cargar formularios</td></tr>';
        }
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
            window.location.replace('./index.html');
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
    window.location.replace('./index.html');
}

// Funciones para modales
function showModal(modalId, data = null) {
    console.log('Opening modal:', modalId);
    const modal = document.getElementById(modalId);
    if (!modal) {
        showToast('Modal no encontrado', 'error');
        return;
    }
    
    // Resetear formulario
    const form = modal.querySelector('form');
    if (form) {
        form.reset();
        
        // Si hay datos, llenar el formulario (modo edición)
        if (data) {
            Object.keys(data).forEach(key => {
                const input = form.querySelector(`[name="${key}"]`);
                if (input) {
                    if (input.type === 'checkbox') {
                        input.checked = data[key];
                    } else {
                        input.value = data[key] || '';
                    }
                }
            });
            
            // Actualizar título del modal
            const title = modal.querySelector('h3');
            if (title) {
                if (modalId === 'userModal') title.textContent = 'Editar Usuario';
                else if (modalId === 'personModal') title.textContent = 'Editar Persona';
                else if (modalId === 'roleModal') title.textContent = 'Editar Rol';
            }
            
            // Para usuarios, hacer la contraseña opcional en modo edición
            if (modalId === 'userModal') {
                const passwordField = form.querySelector('#userPassword');
                if (passwordField) {
                    passwordField.required = false;
                    passwordField.placeholder = 'Dejar en blanco para mantener la actual';
                }
            }
        } else {
            // Modo creación - resetear título
            const title = modal.querySelector('h3');
            if (title) {
                if (modalId === 'userModal') title.textContent = 'Nuevo Usuario';
                else if (modalId === 'personModal') title.textContent = 'Nueva Persona';
                else if (modalId === 'roleModal') title.textContent = 'Nuevo Rol';
            }
            
            // Para usuarios, hacer la contraseña obligatoria en modo creación
            if (modalId === 'userModal') {
                const passwordField = form.querySelector('#userPassword');
                if (passwordField) {
                    passwordField.required = true;
                    passwordField.placeholder = '';
                }
            }
        }
    }
    
    modal.classList.add('active');
    document.body.classList.add('modal-open');
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('active');
        document.body.classList.remove('modal-open');
        
        // Resetear formulario
        const form = modal.querySelector('form');
        if (form) {
            form.reset();
        }
    }
}

// Funciones CRUD para Usuarios
async function editUser(userId) {
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/User/${userId}`);
        if (response && response.ok) {
            const user = await response.json();
            showModal('userModal', user);
        } else {
            showToast('Error al obtener datos del usuario', 'error');
        }
    } catch (error) {
        console.error('Error fetching user:', error);
        showToast('Error al obtener datos del usuario', 'error');
    }
}

async function deleteUser(userId) {
    showConfirmDialog('¿Estás seguro de que quieres eliminar este usuario?', async () => {
        try {
            const response = await authenticatedFetch(`${API_BASE_URL}/User/${userId}`, {
                method: 'DELETE'
            });
            
            if (response && response.ok) {
                showToast('Usuario eliminado correctamente', 'success');
                await loadUsers();
                await loadStats();
            } else {
                showToast('Error al eliminar usuario', 'error');
            }
        } catch (error) {
            console.error('Error deleting user:', error);
            showToast('Error al eliminar usuario', 'error');
        }
    });
}

// Funciones CRUD para Personas
async function editPerson(personId) {
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Person/${personId}`);
        if (response && response.ok) {
            const person = await response.json();
            showModal('personModal', person);
        } else {
            showToast('Error al obtener datos de la persona', 'error');
        }
    } catch (error) {
        console.error('Error fetching person:', error);
        showToast('Error al obtener datos de la persona', 'error');
    }
}

async function deletePerson(personId) {
    showConfirmDialog('¿Estás seguro de que quieres eliminar esta persona?', async () => {
        try {
            const response = await authenticatedFetch(`${API_BASE_URL}/Person/${personId}`, {
                method: 'DELETE'
            });
            
            if (response && response.ok) {
                showToast('Persona eliminada correctamente', 'success');
                await loadPersons();
                await loadStats();
            } else {
                showToast('Error al eliminar persona', 'error');
            }
        } catch (error) {
            console.error('Error deleting person:', error);
            showToast('Error al eliminar persona', 'error');
        }
    });
}

// Funciones CRUD para Roles
async function editRole(roleId) {
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Rol/${roleId}`);
        if (response && response.ok) {
            const role = await response.json();
            showModal('roleModal', role);
        } else {
            showToast('Error al obtener datos del rol', 'error');
        }
    } catch (error) {
        console.error('Error fetching role:', error);
        showToast('Error al obtener datos del rol', 'error');
    }
}

async function deleteRole(roleId) {
    showConfirmDialog('¿Estás seguro de que quieres eliminar este rol?', async () => {
        try {
            const response = await authenticatedFetch(`${API_BASE_URL}/Rol/${roleId}`, {
                method: 'DELETE'
            });
            
            if (response && response.ok) {
                showToast('Rol eliminado correctamente', 'success');
                await loadRoles();
                await loadStats();
            } else {
                showToast('Error al eliminar rol', 'error');
            }
        } catch (error) {
            console.error('Error deleting role:', error);
            showToast('Error al eliminar rol', 'error');
        }
    });
}

// Funciones CRUD para Formularios
async function editForm(formId) {
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Form/${formId}`);
        if (response && response.ok) {
            const form = await response.json();
            showToast(`Editando formulario: ${form.name}`, 'info');
        }
    } catch (error) {
        console.error('Error fetching form:', error);
        showToast('Error al obtener datos del formulario', 'error');
    }
}

async function deleteForm(formId) {
    showConfirmDialog('¿Estás seguro de que quieres eliminar este formulario?', async () => {
        try {
            const response = await authenticatedFetch(`${API_BASE_URL}/Form/${formId}`, {
                method: 'DELETE'
            });
            
            if (response && response.ok) {
                showToast('Formulario eliminado correctamente', 'success');
                await loadForms();
                await loadStats();
            } else {
                showToast('Error al eliminar formulario', 'error');
            }
        } catch (error) {
            console.error('Error deleting form:', error);
            showToast('Error al eliminar formulario', 'error');
        }
    });
}

// Cargar datos de permisos y módulos
async function loadPermissions() {
    try {
        console.log('Loading permissions...');
        const response = await authenticatedFetch(`${API_BASE_URL}/Permission`);
        const tableBody = document.querySelector('#permissionsTable tbody');
        
        if (!tableBody) return;
        
        if (!response || !response.ok) {
            tableBody.innerHTML = '<tr><td colspan=\"5\" style=\"text-align: center; padding: 2rem; color: #dc2626;\">Error al cargar permisos</td></tr>';
            return;
        }
        
        const permissions = await response.json();
        
        if (permissions.length === 0) {
            tableBody.innerHTML = '<tr><td colspan=\"5\" style=\"text-align: center; padding: 2rem; color: #6b7280;\">No hay permisos registrados</td></tr>';
            return;
        }
        
        tableBody.innerHTML = permissions.map(permission => `
            <tr>
                <td>${permission.id}</td>
                <td>${permission.name}</td>
                <td>${permission.code || 'N/A'}</td>
                <td><span class="status-badge ${permission.active ? 'active' : 'inactive'}">${permission.active ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn-icon" onclick="editPermission(${permission.id})" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon danger" onclick="deletePermission(${permission.id})" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');
        
        console.log('Permissions loaded:', permissions.length);
        
    } catch (error) {
        console.error('Error loading permissions:', error);
        const tableBody = document.querySelector('#permissionsTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan=\"5\" style=\"text-align: center; padding: 2rem; color: #dc2626;\">Error al cargar permisos</td></tr>';
        }
    }
}

async function loadModules() {
    try {
        console.log('Loading modules...');
        const response = await authenticatedFetch(`${API_BASE_URL}/Module`);
        const tableBody = document.querySelector('#modulesTable tbody');
        
        if (!tableBody) return;
        
        if (!response || !response.ok) {
            tableBody.innerHTML = '<tr><td colspan=\"4\" style=\"text-align: center; padding: 2rem; color: #dc2626;\">Error al cargar módulos</td></tr>';
            return;
        }
        
        const modules = await response.json();
        
        if (modules.length === 0) {
            tableBody.innerHTML = '<tr><td colspan=\"4\" style=\"text-align: center; padding: 2rem; color: #6b7280;\">No hay módulos registrados</td></tr>';
            return;
        }
        
        tableBody.innerHTML = modules.map(module => `
            <tr>
                <td>${module.id}</td>
                <td>${module.name}</td>
                <td>${module.description || 'N/A'}</td>
                <td><span class="status-badge ${module.active ? 'active' : 'inactive'}">${module.active ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn-icon" onclick="editModule(${module.id})" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon danger" onclick="deleteModule(${module.id})" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');
        
        console.log('Modules loaded:', modules.length);
        
    } catch (error) {
        console.error('Error loading modules:', error);
        const tableBody = document.querySelector('#modulesTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan=\"4\" style=\"text-align: center; padding: 2rem; color: #dc2626;\">Error al cargar módulos</td></tr>';
        }
    }
}

// Funciones para permisos y módulos
async function editPermission(permissionId) {
    showToast('Función de editar permiso en desarrollo', 'info');
}

async function deletePermission(permissionId) {
    showConfirmDialog('¿Estás seguro de que quieres eliminar este permiso?', async () => {
        try {
            const response = await authenticatedFetch(`${API_BASE_URL}/Permission/${permissionId}`, {
                method: 'DELETE'
            });
            
            if (response && response.ok) {
                showToast('Permiso eliminado correctamente', 'success');
                await loadPermissions();
            } else {
                showToast('Error al eliminar permiso', 'error');
            }
        } catch (error) {
            console.error('Error deleting permission:', error);
            showToast('Error al eliminar permiso', 'error');
        }
    });
}

async function editModule(moduleId) {
    showToast('Función de editar módulo en desarrollo', 'info');
}

async function deleteModule(moduleId) {
    showConfirmDialog('¿Estás seguro de que quieres eliminar este módulo?', async () => {
        try {
            const response = await authenticatedFetch(`${API_BASE_URL}/Module/${moduleId}`, {
                method: 'DELETE'
            });
            
            if (response && response.ok) {
                showToast('Módulo eliminado correctamente', 'success');
                await loadModules();
            } else {
                showToast('Error al eliminar módulo', 'error');
            }
        } catch (error) {
            console.error('Error deleting module:', error);
            showToast('Error al eliminar módulo', 'error');
        }
    });
}

// Funciones para manejo de formularios
document.addEventListener('DOMContentLoaded', function() {
    // Form handlers
    const userForm = document.getElementById('userForm');
    const personForm = document.getElementById('personForm');
    const roleForm = document.getElementById('roleForm');
    
    if (userForm) {
        userForm.addEventListener('submit', handleUserSubmit);
    }
    
    if (personForm) {
        personForm.addEventListener('submit', handlePersonSubmit);
    }
    
    if (roleForm) {
        roleForm.addEventListener('submit', handleRoleSubmit);
    }
});

async function handleUserSubmit(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const userData = {
        userName: formData.get('userName'),
        code: formData.get('code'),
        active: formData.get('active') === 'on'
    };
    
    const userId = formData.get('id');
    const isEditing = userId && userId !== '';
    
    // Solo incluir contraseña si está presente
    const password = formData.get('password');
    if (password && password.trim() !== '') {
        userData.password = password;
    } else if (!isEditing) {
        showToast('La contraseña es requerida para nuevos usuarios', 'error');
        return;
    }
    
    try {
        let response;
        if (isEditing) {
            userData.id = parseInt(userId);
            response = await authenticatedFetch(`${API_BASE_URL}/User/${userId}`, {
                method: 'PUT',
                body: JSON.stringify(userData)
            });
        } else {
            response = await authenticatedFetch(`${API_BASE_URL}/User`, {
                method: 'POST',
                body: JSON.stringify(userData)
            });
        }
        
        if (response && response.ok) {
            showToast(`Usuario ${isEditing ? 'actualizado' : 'creado'} correctamente`, 'success');
            closeModal('userModal');
            await loadUsers();
            await loadStats();
        } else {
            const errorData = await response.json().catch(() => ({}));
            showToast(errorData.message || `Error al ${isEditing ? 'actualizar' : 'crear'} usuario`, 'error');
        }
    } catch (error) {
        console.error('Error submitting user:', error);
        showToast(`Error al ${isEditing ? 'actualizar' : 'crear'} usuario`, 'error');
    }
}

async function handlePersonSubmit(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const personData = {
        firstName: formData.get('firstName'),
        lastName: formData.get('lastName'),
        email: formData.get('email'),
        phone: formData.get('phone')
    };
    
    const personId = formData.get('id');
    const isEditing = personId && personId !== '';
    
    try {
        let response;
        if (isEditing) {
            personData.id = parseInt(personId);
            response = await authenticatedFetch(`${API_BASE_URL}/Person/${personId}`, {
                method: 'PUT',
                body: JSON.stringify(personData)
            });
        } else {
            response = await authenticatedFetch(`${API_BASE_URL}/Person`, {
                method: 'POST',
                body: JSON.stringify(personData)
            });
        }
        
        if (response && response.ok) {
            showToast(`Persona ${isEditing ? 'actualizada' : 'creada'} correctamente`, 'success');
            closeModal('personModal');
            await loadPersons();
            await loadStats();
        } else {
            const errorData = await response.json().catch(() => ({}));
            showToast(errorData.message || `Error al ${isEditing ? 'actualizar' : 'crear'} persona`, 'error');
        }
    } catch (error) {
        console.error('Error submitting person:', error);
        showToast(`Error al ${isEditing ? 'actualizar' : 'crear'} persona`, 'error');
    }
}

async function handleRoleSubmit(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const roleData = {
        name: formData.get('name'),
        description: formData.get('description'),
        active: formData.get('active') === 'on'
    };
    
    const roleId = formData.get('id');
    const isEditing = roleId && roleId !== '';
    
    try {
        let response;
        if (isEditing) {
            roleData.id = parseInt(roleId);
            response = await authenticatedFetch(`${API_BASE_URL}/Rol/${roleId}`, {
                method: 'PUT',
                body: JSON.stringify(roleData)
            });
        } else {
            response = await authenticatedFetch(`${API_BASE_URL}/Rol`, {
                method: 'POST',
                body: JSON.stringify(roleData)
            });
        }
        
        if (response && response.ok) {
            showToast(`Rol ${isEditing ? 'actualizado' : 'creado'} correctamente`, 'success');
            closeModal('roleModal');
            await loadRoles();
            await loadStats();
        } else {
            const errorData = await response.json().catch(() => ({}));
            showToast(errorData.message || `Error al ${isEditing ? 'actualizar' : 'crear'} rol`, 'error');
        }
    } catch (error) {
        console.error('Error submitting role:', error);
        showToast(`Error al ${isEditing ? 'actualizar' : 'crear'} rol`, 'error');
    }
}

// Exportar funciones para uso global
window.logout = logout;
window.authenticatedFetch = authenticatedFetch;
window.showModal = showModal;
window.closeModal = closeModal;
window.editUser = editUser;
window.deleteUser = deleteUser;
window.editPerson = editPerson;
window.deletePerson = deletePerson;
window.editRole = editRole;
window.deleteRole = deleteRole;
window.editForm = editForm;
window.deleteForm = deleteForm;
window.editPermission = editPermission;
window.deletePermission = deletePermission;
window.editModule = editModule;
window.deleteModule = deleteModule;
// Role-Based Access Control (RBAC) utilities
let currentUserData = null;

// Inicializar RBAC al cargar la página
document.addEventListener('DOMContentLoaded', function() {
    initializeRBAC();
});

// Inicializar sistema de roles
function initializeRBAC() {
    // Obtener datos del usuario desde sessionStorage
    const userData = sessionStorage.getItem('userData');
    if (userData) {
        try {
            currentUserData = JSON.parse(userData);
            console.log('RBAC initialized for user:', currentUserData.userName);
            console.log('User roles:', currentUserData.roles);
            console.log('User permissions:', currentUserData.permissions);
        } catch (error) {
            console.error('Error parsing user data:', error);
        }
    }
}

// Verificar si el usuario tiene un rol específico
function hasRole(roleName) {
    if (!currentUserData || !currentUserData.roles) {
        return false;
    }
    return currentUserData.roles.includes(roleName);
}

// Verificar si el usuario tiene múltiples roles
function hasAnyRole(roleNames) {
    if (!currentUserData || !currentUserData.roles) {
        return false;
    }
    return roleNames.some(role => currentUserData.roles.includes(role));
}

// Verificar si el usuario tiene todos los roles especificados
function hasAllRoles(roleNames) {
    if (!currentUserData || !currentUserData.roles) {
        return false;
    }
    return roleNames.every(role => currentUserData.roles.includes(role));
}

// Verificar si el usuario tiene un permiso específico
function hasPermission(permissionName) {
    if (!currentUserData || !currentUserData.permissions) {
        return false;
    }
    return currentUserData.permissions.includes(permissionName);
}

// Verificar si el usuario tiene alguno de los permisos especificados
function hasAnyPermission(permissionNames) {
    if (!currentUserData || !currentUserData.permissions) {
        return false;
    }
    return permissionNames.some(permission => currentUserData.permissions.includes(permission));
}

// Verificar si el usuario es administrador
function isAdmin() {
    return hasRole('Admin') || hasRole('Administrator') || hasRole('SuperAdmin');
}

// Verificar si el usuario es un usuario normal
function isUser() {
    return hasRole('User') && !isAdmin();
}

// Ocultar/mostrar elementos basado en roles
function applyRoleBasedVisibility() {
    // Ocultar elementos para usuarios normales
    const adminOnlyElements = document.querySelectorAll('[data-role="admin"], [data-admin-only="true"]');
    adminOnlyElements.forEach(element => {
        if (!isAdmin()) {
            element.style.display = 'none';
        }
    });

    // Mostrar elementos solo para usuarios normales
    const userOnlyElements = document.querySelectorAll('[data-role="user"], [data-user-only="true"]');
    userOnlyElements.forEach(element => {
        if (!isUser()) {
            element.style.display = 'none';
        }
    });

    // Ocultar elementos basado en permisos específicos
    document.querySelectorAll('[data-permission]').forEach(element => {
        const requiredPermission = element.getAttribute('data-permission');
        if (!hasPermission(requiredPermission)) {
            element.style.display = 'none';
        }
    });

    // Ocultar elementos basado en roles específicos
    document.querySelectorAll('[data-required-role]').forEach(element => {
        const requiredRole = element.getAttribute('data-required-role');
        if (!hasRole(requiredRole)) {
            element.style.display = 'none';
        }
    });
}

// Deshabilitar botones basado en permisos
function applyRoleBasedDisabling() {
    // Deshabilitar botones de eliminación para usuarios normales
    const deleteButtons = document.querySelectorAll('.btn-icon.danger, [data-action="delete"]');
    deleteButtons.forEach(button => {
        if (!isAdmin() && !hasPermission('Delete')) {
            button.disabled = true;
            button.style.opacity = '0.5';
            button.style.cursor = 'not-allowed';
            button.title = 'No tienes permisos para esta acción';
        }
    });

    // Deshabilitar botones de edición si no tiene permisos
    const editButtons = document.querySelectorAll('[data-action="edit"], .btn-icon:not(.danger)');
    editButtons.forEach(button => {
        if (!isAdmin() && !hasPermission('Edit')) {
            button.disabled = true;
            button.style.opacity = '0.5';
            button.style.cursor = 'not-allowed';
            button.title = 'No tienes permisos para esta acción';
        }
    });

    // Deshabilitar botones de creación
    const createButtons = document.querySelectorAll('.btn-primary[onclick*="showModal"], [data-action="create"]');
    createButtons.forEach(button => {
        if (!isAdmin() && !hasPermission('Create')) {
            button.disabled = true;
            button.style.opacity = '0.5';
            button.style.cursor = 'not-allowed';
            button.title = 'No tienes permisos para esta acción';
        }
    });
}

// Filtrar secciones del menú basado en roles
function filterNavigationByRole() {
    const navSections = document.querySelectorAll('.nav-section');
    
    navSections.forEach(section => {
        const sectionTitle = section.querySelector('h4').textContent.toLowerCase();
        
        // Si el usuario no es admin, ocultar secciones administrativas
        if (!isAdmin()) {
            // Ocultar gestión completa para usuarios normales
            if (sectionTitle.includes('gestión')) {
                section.style.display = 'none';
            }
            
            // Permitir solo vista general para usuarios normales
            const links = section.querySelectorAll('.nav-link');
            links.forEach(link => {
                const linkSection = link.getAttribute('data-section');
                
                // Permitir solo overview para usuarios normales
                if (linkSection !== 'overview') {
                    link.style.display = 'none';
                }
            });
        }
    });

    // Ocultar secciones individuales basado en permisos
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        const section = link.getAttribute('data-section');
        
        // Mapear secciones a permisos requeridos
        const sectionPermissions = {
            'users': 'ManageUsers',
            'persons': 'ManagePersons', 
            'roles': 'ManageRoles',
            'permissions': 'ManagePermissions',
            'forms': 'ManageForms',
            'modules': 'ManageModules'
        };

        if (sectionPermissions[section] && !hasPermission(sectionPermissions[section]) && !isAdmin()) {
            link.style.display = 'none';
        }
    });
}

// Verificar autorización para una acción específica
function checkAuthorization(action, resource = null) {
    // Acciones que requieren permisos de admin
    const adminActions = ['delete', 'create', 'edit', 'manage'];
    
    if (adminActions.includes(action.toLowerCase())) {
        return isAdmin() || hasPermission(`${action}${resource ? resource.charAt(0).toUpperCase() + resource.slice(1) : ''}`);
    }
    
    // Acción de lectura - permitida para todos los usuarios autenticados
    if (action.toLowerCase() === 'read' || action.toLowerCase() === 'view') {
        return true;
    }
    
    return false;
}

// Interceptar acciones y verificar permisos
function interceptAction(action, callback, errorMessage = 'No tienes permisos para realizar esta acción') {
    return function(...args) {
        if (checkAuthorization(action)) {
            return callback.apply(this, args);
        } else {
            showToast(errorMessage, 'error');
            console.warn(`Acción bloqueada: ${action}. Usuario no autorizado.`);
            return false;
        }
    };
}

// Aplicar todas las restricciones de RBAC
function applyRoleBasedAccessControl() {
    if (!currentUserData) {
        console.warn('No user data found, redirecting to login');
        window.location.replace('./index.html');
        return;
    }

    console.log('Applying RBAC restrictions...');
    
    applyRoleBasedVisibility();
    applyRoleBasedDisabling();
    filterNavigationByRole();
    
    console.log('RBAC restrictions applied successfully');
}

// Actualizar datos del usuario (llamar después del login)
function updateUserData(userData) {
    currentUserData = userData;
    sessionStorage.setItem('userData', JSON.stringify(userData));
    console.log('User data updated:', userData);
}

// Obtener información del usuario actual
function getCurrentUser() {
    return currentUserData;
}

// Limpiar datos de usuario (llamar en logout)
function clearUserData() {
    currentUserData = null;
    sessionStorage.removeItem('userData');
}

// Exportar funciones para uso global
window.hasRole = hasRole;
window.hasAnyRole = hasAnyRole;
window.hasAllRoles = hasAllRoles;
window.hasPermission = hasPermission;
window.hasAnyPermission = hasAnyPermission;
window.isAdmin = isAdmin;
window.isUser = isUser;
window.checkAuthorization = checkAuthorization;
window.interceptAction = interceptAction;
window.applyRoleBasedAccessControl = applyRoleBasedAccessControl;
window.updateUserData = updateUserData;
window.getCurrentUser = getCurrentUser;
window.clearUserData = clearUserData;
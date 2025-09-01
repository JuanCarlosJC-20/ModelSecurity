// Configuración dinámica para diferentes entornos
const CONFIG = {
    // Detectar si estamos en contenedor Docker
    getApiUrl: function() {
        // Si existe la variable API_URL del contenedor, usarla
        if (window.API_URL) {
            return window.API_URL;
        }
        
        // Si estamos en localhost, usar localhost
        if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
            return 'http://localhost:5000/api';
        }
        
        // Para producción, usar la misma IP/dominio que el frontend pero puerto 5000
        return `http://${window.location.hostname}:5000/api`;
    }
};

// Variable global para usar en otros archivos JS
window.API_BASE_URL = CONFIG.getApiUrl();
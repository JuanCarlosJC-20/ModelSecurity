#!/bin/bash

# Script de despliegue para Ubuntu
# Ejecutar con: chmod +x deploy-ubuntu.sh && ./deploy-ubuntu.sh

set -e

echo "🚀 Iniciando despliegue en Ubuntu..."

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para mostrar mensajes
show_message() {
    echo -e "${GREEN}✅ $1${NC}"
}

show_warning() {
    echo -e "${YELLOW}⚠️ $1${NC}"
}

show_error() {
    echo -e "${RED}❌ $1${NC}"
}

show_info() {
    echo -e "${BLUE}ℹ️ $1${NC}"
}

# Verificar que estamos en el directorio correcto
if [ ! -f "Backend/docker-compose.yml" ]; then
    show_error "No se encontró Backend/docker-compose.yml. Ejecuta este script desde la raíz del proyecto ModelSecurity"
    exit 1
fi

show_info "Verificando dependencias..."

# Verificar Docker
if ! command -v docker &> /dev/null; then
    show_error "Docker no está instalado. Instalando..."
    sudo apt update
    sudo apt install -y docker.io docker-compose
    sudo systemctl enable docker
    sudo systemctl start docker
    sudo usermod -aG docker $USER
    show_warning "Docker instalado. Reinicia la sesión o ejecuta: newgrp docker"
fi

# Verificar Docker Compose
if ! command -v docker-compose &> /dev/null; then
    show_error "Docker Compose no está instalado. Instalando..."
    sudo apt install -y docker-compose
fi

show_message "Docker y Docker Compose están disponibles"

# Preguntar por la IP del servidor
read -p "📡 Ingresa la IP de tu servidor Ubuntu (o presiona Enter para usar localhost): " SERVER_IP
SERVER_IP=${SERVER_IP:-localhost}

show_info "Configurando variables de entorno para IP: $SERVER_IP"

# Actualizar archivo .env
cd Backend
cat > .env << EOF
# Configuración para producción en Ubuntu
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ASPNETCORE_HTTP_PORTS=8080

# URLs del sistema
FRONTEND_URL=http://$SERVER_IP:3000
API_URL=http://$SERVER_IP:5000

# Base de datos
DB_TYPE=mysql
MYSQL_ROOT_PASSWORD=1234567
MYSQL_DATABASE=ModelSecurity

# Configuración JWT
JWT_KEY=EsteEsUnSecretoSuperSeguroDeMasDe32Caracteres!!
JWT_ISSUER=MiApi
JWT_AUDIENCE=MiApiUsuarios

# Configuración de red
NETWORK_NAME=modelsecurity-network
EOF

show_message "Archivo .env actualizado"

# Detener contenedores existentes si existen
show_info "Deteniendo contenedores existentes..."
docker-compose down --remove-orphans 2>/dev/null || true

# Limpiar imágenes anteriores
show_info "Limpiando imágenes anteriores..."
docker system prune -f

# Construir y levantar servicios
show_info "Construyendo imágenes..."
docker-compose build --no-cache

show_info "Iniciando servicios..."
docker-compose up -d

# Esperar a que los servicios estén listos
show_info "Esperando a que los servicios estén listos..."
sleep 30

# Verificar estado de los contenedores
show_info "Verificando estado de los servicios..."
docker-compose ps

# Verificar logs
show_info "Verificando logs del API..."
docker-compose logs modelsecurity-api --tail=20

# Mostrar información final
show_message "🎉 ¡Despliegue completado!"
echo ""
show_info "Accede a tu aplicación en:"
echo "   🌐 Frontend: http://$SERVER_IP:3000"
echo "   🔧 API: http://$SERVER_IP:5000"
echo "   🗄️ MySQL: $SERVER_IP:3306"
echo ""
show_info "Comandos útiles:"
echo "   📊 Ver logs: docker-compose logs -f"
echo "   🔄 Reiniciar: docker-compose restart"
echo "   ⏹️ Detener: docker-compose down"
echo "   📈 Estado: docker-compose ps"
echo ""
show_warning "Configura tu firewall para permitir los puertos 3000 y 5000"
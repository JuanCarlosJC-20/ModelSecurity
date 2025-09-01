#!/bin/bash
# deploy.sh - Script de despliegue para Ubuntu

set -e

echo "🚀 Iniciando despliegue en Ubuntu..."

# Verificar que Docker esté instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker no está instalado. Instalando..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    echo "✅ Docker instalado. Reinicia la sesión para aplicar los cambios."
    exit 1
fi

# Verificar que Docker Compose esté instalado
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose no está instalado. Instalando..."
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    echo "✅ Docker Compose instalado."
fi

# Crear directorios necesarios
echo "📁 Creando directorios..."
mkdir -p nginx/ssl mysql/conf.d postgres/init

# Generar certificados SSL autofirmados si no existen
if [ ! -f nginx/ssl/server.crt ]; then
    echo "🔐 Generando certificados SSL..."
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout nginx/ssl/server.key \
        -out nginx/ssl/server.crt \
        -subj "/C=CO/ST=Bogota/L=Bogota/O=ModelSecurity/CN=localhost"
fi

# Configurar permisos correctos
sudo chown -R $USER:$USER .
chmod +x *.sh

# Verificar archivo .env
if [ ! -f .env ]; then
    echo "⚠️  Archivo .env no encontrado. Creando uno de ejemplo..."
    cp .env.example .env 2>/dev/null || echo "Crea manualmente el archivo .env"
fi

# Detener contenedores existentes
echo "🛑 Deteniendo contenedores existentes..."
docker-compose down -v 2>/dev/null || true

# Limpiar imágenes anteriores (opcional)
echo "🧹 Limpiando imágenes anteriores..."
docker system prune -f

# Construir y levantar servicios
echo "🔨 Construyendo y levantando servicios..."
docker-compose up -d --build

# Esperar a que los servicios estén listos
echo "⏳ Esperando a que los servicios estén listos..."
sleep 30

# Verificar estado de los servicios
echo "🔍 Verificando estado de los servicios..."
docker-compose ps

# Mostrar logs si hay errores
if ! docker-compose ps | grep -q "Up"; then
    echo "❌ Algunos servicios no se iniciaron correctamente. Mostrando logs:"
    docker-compose logs --tail=50
    exit 1
fi

# Ejecutar migraciones (si existen)
echo "🗄️  Ejecutando migraciones de base de datos..."
docker-compose exec -T modelsecurity-api dotnet ef database update 2>/dev/null || echo "⚠️  No se encontraron migraciones o hubo un error"

echo "✅ Despliegue completado!"
echo ""
echo "📋 URLs disponibles:"
echo "   Frontend: http://localhost:3000"
echo "   API: http://localhost:5000"
echo "   Nginx (si configurado): http://localhost"
echo ""
echo "🗄️  Bases de datos disponibles:"
echo "   MySQL: localhost:3306"
echo "   PostgreSQL: localhost:5432" 
echo "   SQL Server: localhost:1433"
echo ""
echo "🔧 Para ver logs: docker-compose logs -f"
echo "🛑 Para detener: docker-compose down"
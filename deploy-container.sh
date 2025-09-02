#!/bin/bash

# Script de despliegue para CONTENEDOR Docker (MySQL + PostgreSQL)
# Ejecutar con: chmod +x deploy-container.sh && ./deploy-container.sh

set -e

echo "🚀 Iniciando despliegue en CONTENEDOR Docker..."

# Colores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

show_message() { echo -e "${GREEN}✅ $1${NC}"; }
show_info() { echo -e "${BLUE}ℹ️ $1${NC}"; }
show_warning() { echo -e "${YELLOW}⚠️ $1${NC}"; }

# Verificar directorio
if [ ! -f "Backend/Web/Web.csproj" ]; then
    echo "❌ No se encontró Backend/Web/Web.csproj"
    exit 1
fi

# Actualizar sistema
show_info "Actualizando sistema..."
apt update && apt upgrade -y

# Instalar .NET 8
show_info "Instalando .NET 8..."
if ! command -v dotnet &> /dev/null; then
    # Detectar versión de Ubuntu y usar la correcta
    UBUNTU_VERSION="22.04"
    if [ -f /etc/lsb-release ]; then
        UBUNTU_VERSION=$(grep DISTRIB_RELEASE /etc/lsb-release | cut -d'=' -f2)
    fi
    
    # Si es Ubuntu 24.04, usar 22.04 como fallback para .NET 8
    if [ "$UBUNTU_VERSION" = "24.04" ]; then
        UBUNTU_VERSION="22.04"
    fi
    
    wget https://packages.microsoft.com/config/ubuntu/$UBUNTU_VERSION/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    apt update
    
    # Instalar .NET 8 con fallback
    if ! apt install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0; then
        # Fallback: instalación manual si falla el paquete
        wget https://download.visualstudio.microsoft.com/download/pr/b8cf2dd4-6ecf-4cb5-9c21-63b4a4c63ccc/7d57b75a2ef4d6a0f4bfa8d02b2e5f90/dotnet-sdk-8.0.403-linux-x64.tar.gz
        mkdir -p /usr/share/dotnet
        tar zxf dotnet-sdk-8.0.403-linux-x64.tar.gz -C /usr/share/dotnet
        ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet
        rm dotnet-sdk-8.0.403-linux-x64.tar.gz
    fi
fi
show_message ".NET 8 instalado"

# Instalar MySQL
show_info "Instalando MySQL..."
if ! command -v mysql &> /dev/null; then
    DEBIAN_FRONTEND=noninteractive apt install -y mysql-server mysql-client
    service mysql start
    
    # Configurar MySQL
    mysql -e "CREATE DATABASE IF NOT EXISTS ModelSecurity;"
    mysql -e "CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY '1234567';"
    mysql -e "GRANT ALL PRIVILEGES ON ModelSecurity.* TO 'root'@'%';"
    mysql -e "FLUSH PRIVILEGES;"
fi
show_message "MySQL configurado"

# Instalar PostgreSQL
show_info "Instalando PostgreSQL..."
if ! command -v psql &> /dev/null; then
    DEBIAN_FRONTEND=noninteractive apt install -y postgresql postgresql-contrib
    service postgresql start
    
    # Configurar PostgreSQL
    sudo -u postgres psql -c "CREATE DATABASE \"ModelSecurity\";" || true
    sudo -u postgres psql -c "CREATE USER postgres WITH PASSWORD '1234567';" || true
    sudo -u postgres psql -c "ALTER USER postgres CREATEDB;" || true
    sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE \"ModelSecurity\" TO postgres;" || true
fi
show_message "PostgreSQL configurado"

# Instalar Nginx
show_info "Instalando Nginx..."
if ! command -v nginx &> /dev/null; then
    apt install -y nginx
fi
show_message "Nginx instalado"

# Obtener IP del contenedor
CONTAINER_IP=$(hostname -I | awk '{print $1}')

# Obtener IP del servidor
read -p "📡 Ingresa la IP de tu servidor (o presiona Enter para usar $CONTAINER_IP): " SERVER_IP
SERVER_IP=${SERVER_IP:-$CONTAINER_IP}
show_info "Configurando para IP: $SERVER_IP"

# Compilar aplicación .NET
show_info "Compilando aplicación .NET..."
cd Backend/Web

cat > appsettings.Production.json << EOF
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=ModelSecurity;User=root;Password=1234567",
    "PostgresDb": "Host=localhost;Port=5432;Database=ModelSecurity;Username=postgres;Password=1234567"
  },
  "Jwt": {
    "Key": "EsteEsUnSecretoSuperSeguroDeMasDe32Caracteres!!",
    "Issuer": "MiApi",
    "Audience": "MiApiUsuarios"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "OrigenesPermitidos": [
    "http://$SERVER_IP:8080",
    "https://$SERVER_IP:8080",
    "http://localhost:8080"
  ]
}
EOF

# Compilar
dotnet restore
dotnet publish -c Release -o /opt/modelsecurity-api
show_message "Aplicación compilada"

# Configurar Frontend
show_info "Configurando Frontend..."
cd ../../frontend

cat > js/config.js << EOF
const CONFIG = {
    getApiUrl: function() {
        return 'http://$SERVER_IP:5000/api';
    }
};
window.API_BASE_URL = CONFIG.getApiUrl();
EOF

# Copiar frontend
cp -r * /var/www/html/

# Configurar nginx para puerto 8080
cat > /etc/nginx/sites-available/default << EOF
server {
    listen 8080;
    server_name $SERVER_IP localhost;
    root /var/www/html;
    index index.html;

    location / {
        try_files \$uri \$uri/ /index.html;
    }

    location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF

show_message "Configuración completada"

# Crear script de inicio
cat > /opt/start-services.sh << EOF
#!/bin/bash

echo "🚀 Iniciando servicios..."

# Iniciar MySQL
service mysql start
echo "✅ MySQL iniciado en puerto 3306"

# Iniciar PostgreSQL
service postgresql start
echo "✅ PostgreSQL iniciado en puerto 5432"

# Esperar a que las bases de datos estén listas
sleep 5

# Iniciar Nginx
service nginx start
echo "✅ Nginx iniciado en puerto 8080"

# Iniciar API en background
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &
echo "✅ API iniciada en puerto 5000"

echo ""
echo "🌐 Aplicación disponible en:"
echo "   Frontend: http://$SERVER_IP:8080"
echo "   API: http://$SERVER_IP:5000"
echo ""
echo "🗄️ Bases de datos disponibles:"
echo "   MySQL: localhost:3306 (root/1234567)"
echo "   PostgreSQL: localhost:5432 (postgres/1234567)"
echo ""
echo "📊 Para ver logs de la API: tail -f /var/log/api.log"
echo "🔄 Para reiniciar servicios: /opt/start-services.sh"
echo "🔍 Para verificar bases de datos:"
echo "   MySQL: mysql -u root -p1234567 -e 'SHOW DATABASES;'"
echo "   PostgreSQL: sudo -u postgres psql -c '\\l'"

# Mantener el contenedor activo
tail -f /var/log/api.log
EOF

chmod +x /opt/start-services.sh

show_message "🎉 Instalación completada!"
echo ""
show_info "Para iniciar todos los servicios, ejecuta:"
echo "   /opt/start-services.sh"
echo ""
show_warning "IMPORTANTE: Solo MySQL y PostgreSQL configurados"
echo "            Ejecuta el script de inicio cada vez que reinicies el contenedor"
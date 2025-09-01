#!/bin/bash

# Script de despliegue para CONTENEDOR Docker (sin systemd)
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

# Instalar .NET 9
show_info "Instalando .NET 9..."
if ! command -v dotnet &> /dev/null; then
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    apt update
    apt install -y dotnet-sdk-9.0 aspnetcore-runtime-9.0
fi
show_message ".NET 9 instalado"

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

# Instalar Nginx
show_info "Instalando Nginx..."
if ! command -v nginx &> /dev/null; then
    apt install -y nginx
fi
show_message "Nginx instalado"

# Obtener IP
read -p "📡 Ingresa la IP de tu servidor (o presiona Enter para localhost): " SERVER_IP
SERVER_IP=${SERVER_IP:-localhost}
show_info "Configurando para IP: $SERVER_IP"

# Compilar aplicación .NET
show_info "Compilando aplicación .NET..."
cd Backend/Web

cat > appsettings.Production.json << EOF
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=ModelSecurity;User=root;Password=1234567"
  },
  "Jwt": {
    "Key": "EsteEsUnSecretoSuperSeguroDeMasDe32Caracteres!!",
    "Issuer": "MiApi",
    "Audience": "MiApiUsuarios"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OrigenesPermitidos": [
    "http://$SERVER_IP:3000",
    "https://$SERVER_IP:3000",
    "http://localhost:3000"
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

# Configurar nginx
cat > /etc/nginx/sites-available/default << EOF
server {
    listen 3000;
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
cat > /opt/start-services.sh << 'EOF'
#!/bin/bash

echo "🚀 Iniciando servicios..."

# Iniciar MySQL
service mysql start
echo "✅ MySQL iniciado"

# Iniciar Nginx
service nginx start
echo "✅ Nginx iniciado"

# Iniciar API en background
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &
echo "✅ API iniciada en puerto 5000"

echo ""
echo "🌐 Aplicación disponible en:"
echo "   Frontend: http://$SERVER_IP:3000"
echo "   API: http://$SERVER_IP:5000"
echo ""
echo "📊 Para ver logs de la API: tail -f /var/log/api.log"
echo "🔄 Para reiniciar servicios: /opt/start-services.sh"

# Mantener el contenedor activo
tail -f /var/log/api.log
EOF

chmod +x /opt/start-services.sh

show_message "🎉 Instalación completada!"
echo ""
show_info "Para iniciar todos los servicios, ejecuta:"
echo "   /opt/start-services.sh"
echo ""
show_warning "IMPORTANTE: Los servicios se ejecutan en modo manual (no systemd)"
echo "            Ejecuta el script de inicio cada vez que reinicies el contenedor"
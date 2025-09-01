#!/bin/bash

# Script de despliegue NATIVO para Ubuntu (sin Docker)
# Ejecutar con: chmod +x deploy-ubuntu-native.sh && ./deploy-ubuntu-native.sh

set -e

echo "🚀 Iniciando despliegue NATIVO en Ubuntu..."

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
if [ ! -f "Backend/Web/Web.csproj" ]; then
    show_error "No se encontró Backend/Web/Web.csproj. Ejecuta este script desde la raíz del proyecto ModelSecurity"
    exit 1
fi

show_info "Actualizando sistema Ubuntu..."
apt update && apt upgrade -y

# Instalar .NET 9
show_info "Instalando .NET 9 SDK..."
if ! command -v dotnet &> /dev/null; then
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    apt update
    apt install -y dotnet-sdk-9.0 aspnetcore-runtime-9.0
fi
show_message ".NET 9 instalado correctamente"

# Instalar MySQL
show_info "Instalando MySQL..."
if ! command -v mysql &> /dev/null; then
    apt install -y mysql-server mysql-client
    systemctl start mysql
    systemctl enable mysql
    
    # Configurar MySQL
    show_info "Configurando MySQL..."
    mysql -e "CREATE DATABASE IF NOT EXISTS ModelSecurity;"
    mysql -e "CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY '1234567';"
    mysql -e "GRANT ALL PRIVILEGES ON ModelSecurity.* TO 'root'@'%';"
    mysql -e "FLUSH PRIVILEGES;"
fi
show_message "MySQL configurado correctamente"

# Instalar Nginx
show_info "Instalando Nginx..."
if ! command -v nginx &> /dev/null; then
    apt install -y nginx
    systemctl start nginx
    systemctl enable nginx
fi
show_message "Nginx instalado correctamente"

# Preguntar por la IP del servidor
read -p "📡 Ingresa la IP de tu servidor Ubuntu: " SERVER_IP
if [ -z "$SERVER_IP" ]; then
    SERVER_IP="localhost"
fi

show_info "Configurando para IP: $SERVER_IP"

# Configurar aplicación .NET
show_info "Compilando aplicación .NET..."
cd Backend/Web

# Crear appsettings.Production.json con IP correcta
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
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "OrigenesPermitidos": [
    "http://$SERVER_IP:3000",
    "https://$SERVER_IP:3000",
    "http://localhost:3000",
    "https://localhost:3000"
  ]
}
EOF

# Compilar la aplicación
dotnet restore
dotnet publish -c Release -o /opt/modelsecurity-api

show_message "Aplicación .NET compilada"

# Configurar servicio systemd para la API
show_info "Configurando servicio de la API..."
cat > /etc/systemd/system/modelsecurity-api.service << EOF
[Unit]
Description=ModelSecurity API
After=network.target mysql.service

[Service]
Type=notify
WorkingDirectory=/opt/modelsecurity-api
ExecStart=/usr/bin/dotnet /opt/modelsecurity-api/Web.dll
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
User=www-data
Group=www-data
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Dar permisos correctos
chown -R www-data:www-data /opt/modelsecurity-api

# Habilitar y iniciar servicio
systemctl daemon-reload
systemctl enable modelsecurity-api
systemctl start modelsecurity-api

show_message "Servicio API configurado"

# Configurar Frontend con Nginx
show_info "Configurando Frontend..."
cd ../../frontend

# Actualizar config.js con la IP correcta
cat > js/config.js << EOF
// Configuración para producción Ubuntu nativo
const CONFIG = {
    getApiUrl: function() {
        return 'http://$SERVER_IP:5000/api';
    }
};

// Variable global para usar en otros archivos JS
window.API_BASE_URL = CONFIG.getApiUrl();
EOF

# Copiar archivos del frontend a nginx
cp -r * /var/www/html/

# Configurar nginx para el frontend
cat > /etc/nginx/sites-available/modelsecurity << EOF
server {
    listen 3000;
    server_name $SERVER_IP localhost;
    root /var/www/html;
    index index.html login.html;

    location / {
        try_files \$uri \$uri/ /index.html;
    }

    location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Configurar CORS para API
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

# Habilitar el sitio
ln -sf /etc/nginx/sites-available/modelsecurity /etc/nginx/sites-enabled/
nginx -t && systemctl reload nginx

show_message "Frontend configurado"

# Configurar firewall
show_info "Configurando firewall..."
if command -v ufw &> /dev/null; then
    ufw allow 3000
    ufw allow 5000
    ufw allow 22
    ufw --force enable
fi

# Verificar servicios
show_info "Verificando servicios..."
systemctl status mysql --no-pager -l
systemctl status modelsecurity-api --no-pager -l
systemctl status nginx --no-pager -l

# Mostrar información final
show_message "🎉 ¡Despliegue NATIVO completado!"
echo ""
show_info "Accede a tu aplicación en:"
echo "   🌐 Frontend: http://$SERVER_IP:3000"
echo "   🔧 API: http://$SERVER_IP:5000"
echo "   🗄️ MySQL: $SERVER_IP:3306"
echo ""
show_info "Comandos útiles:"
echo "   📊 Ver logs API: journalctl -u modelsecurity-api -f"
echo "   🔄 Reiniciar API: systemctl restart modelsecurity-api"
echo "   📈 Estado servicios: systemctl status modelsecurity-api"
echo "   🌐 Reiniciar Nginx: systemctl restart nginx"
echo ""
show_info "Archivos importantes:"
echo "   📁 API: /opt/modelsecurity-api/"
echo "   📁 Frontend: /var/www/html/"
echo "   ⚙️ Config API: /etc/systemd/system/modelsecurity-api.service"
echo "   ⚙️ Config Nginx: /etc/nginx/sites-available/modelsecurity"
# 🚀 Guía de Despliegue NATIVO en Ubuntu

Esta guía te ayudará a desplegar tu aplicación ModelSecurity **directamente en Ubuntu** sin usar Docker.

## 📋 Requisitos Previos

- ✅ **Ubuntu 20.04 o superior**
- ✅ **Acceso root** o sudo en el servidor
- ✅ **Conexión a internet**
- ✅ **Puertos 3000, 5000 y 3306 disponibles**

## 🎯 Paso a Paso

### **Paso 1: Clonar el Repositorio**

```bash
# Si git no está instalado:
apt install -y git

# Clonar el proyecto:
git clone https://github.com/JuanCarlosJC-20/ModelSecurity.git
cd ModelSecurity
```

### **Paso 2: Dar Permisos al Script**

```bash
chmod +x deploy-ubuntu-native.sh
```

### **Paso 3: Ejecutar el Script (Como ROOT)**

```bash
# IMPORTANTE: Ejecutar como root
./deploy-ubuntu-native.sh
```

### **Paso 4: Configurar IP del Servidor**

Cuando el script te pregunte:
```
📡 Ingresa la IP de tu servidor Ubuntu: 
```

**Opción A - Para acceso local:**
```bash
localhost
```

**Opción B - Para acceso externo:**
```bash
# Obtener tu IP primero:
hostname -I
# Luego ingresarla, ejemplo: 192.168.1.100
```

### **Paso 5: Esperar la Instalación**

El script automáticamente:
- ⏳ Instala .NET 9 SDK
- ⏳ Instala MySQL Server
- ⏳ Instala Nginx
- ⏳ Compila tu aplicación
- ⏳ Configura todos los servicios
- ⏳ Abre puertos del firewall

**Tiempo estimado:** 5-10 minutos

## ✅ Verificar que Todo Funciona

### **1. Verificar Servicios**
```bash
# Ver estado de la API:
systemctl status modelsecurity-api

# Ver estado de MySQL:
systemctl status mysql

# Ver estado de Nginx:
systemctl status nginx
```

### **2. Verificar Conectividad**
```bash
# Probar API:
curl http://localhost:5000/api

# Probar Frontend:
curl http://localhost:3000
```

## 🌐 Acceder a la Aplicación

Una vez completado el despliegue:

- **🌐 Frontend**: `http://TU_IP:3000`
- **🔧 API**: `http://TU_IP:5000`
- **🗄️ MySQL**: `TU_IP:3306`

## 🛠️ Comandos Útiles

### **Ver Logs**
```bash
# Logs de la API en tiempo real:
journalctl -u modelsecurity-api -f

# Logs de Nginx:
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log
```

### **Reiniciar Servicios**
```bash
# Reiniciar API:
systemctl restart modelsecurity-api

# Reiniciar Nginx:
systemctl restart nginx

# Reiniciar MySQL:
systemctl restart mysql
```

### **Ver Estado del Sistema**
```bash
# Ver todos los servicios relacionados:
systemctl status modelsecurity-api mysql nginx

# Ver puertos abiertos:
netstat -tulpn | grep -E ':(3000|5000|3306)'
```

## 📁 Ubicaciones Importantes

### **Archivos de la Aplicación**
- **API**: `/opt/modelsecurity-api/`
- **Frontend**: `/var/www/html/`
- **Configuración API**: `/etc/systemd/system/modelsecurity-api.service`
- **Configuración Nginx**: `/etc/nginx/sites-available/modelsecurity`

### **Logs del Sistema**
- **API**: `journalctl -u modelsecurity-api`
- **Nginx**: `/var/log/nginx/`
- **MySQL**: `/var/log/mysql/`

## 🔧 Personalización

### **Cambiar Puerto de la API**
```bash
# Editar el servicio:
nano /etc/systemd/system/modelsecurity-api.service

# Cambiar la línea:
Environment=ASPNETCORE_URLS=http://0.0.0.0:TU_PUERTO

# Recargar y reiniciar:
systemctl daemon-reload
systemctl restart modelsecurity-api
```

### **Cambiar Puerto del Frontend**
```bash
# Editar configuración de Nginx:
nano /etc/nginx/sites-available/modelsecurity

# Cambiar la línea:
listen TU_PUERTO;

# Reiniciar Nginx:
systemctl restart nginx
```

## 🐛 Solución de Problemas

### **API no inicia**
```bash
# Ver logs detallados:
journalctl -u modelsecurity-api --no-pager -l

# Verificar archivo de configuración:
cat /opt/modelsecurity-api/appsettings.Production.json
```

### **MySQL no conecta**
```bash
# Probar conexión manual:
mysql -u root -p1234567 -e "SHOW DATABASES;"

# Reiniciar MySQL:
systemctl restart mysql
```

### **Frontend no carga**
```bash
# Verificar archivos:
ls -la /var/www/html/

# Verificar configuración Nginx:
nginx -t

# Ver logs de Nginx:
tail -f /var/log/nginx/error.log
```

### **Puertos no accesibles desde otras máquinas**
```bash
# Verificar firewall:
ufw status

# Abrir puertos manualmente:
ufw allow 3000
ufw allow 5000
```

## 🔒 Seguridad para Producción

### **Cambiar Contraseñas por Defecto**
```bash
# Cambiar contraseña de MySQL:
mysql -u root -p -e "ALTER USER 'root'@'localhost' IDENTIFIED BY 'TU_NUEVA_CONTRASEÑA';"

# Actualizar en appsettings.Production.json:
nano /opt/modelsecurity-api/appsettings.Production.json
```

### **Configurar HTTPS (Opcional)**
```bash
# Instalar Certbot:
apt install -y certbot python3-certbot-nginx

# Obtener certificado SSL:
certbot --nginx -d tu-dominio.com
```

## ✅ ¡Listo!

Tu aplicación ModelSecurity está ejecutándose **nativamente en Ubuntu** y lista para usar.

### **URLs de Acceso:**
- Frontend: `http://TU_IP:3000`
- API: `http://TU_IP:5000`

### **Credenciales por Defecto:**
- MySQL: usuario `root`, contraseña `1234567`
- Base de datos: `ModelSecurity`
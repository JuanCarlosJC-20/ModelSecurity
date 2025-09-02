# 🌐 ModelSecurity - Configuración para Acceso Externo

Guía completa para permitir que **otras máquinas** de la red accedan a tu aplicación ModelSecurity.

## 📋 ¿Qué Vamos a Lograr?

- ✅ Acceso desde cualquier PC de la red local
- ✅ Frontend accesible desde: `http://TU_IP:3000`
- ✅ API accesible desde: `http://TU_IP:5000`
- ✅ Configuración de CORS para múltiples orígenes
- ✅ Configuración de firewall de Windows

---

## 🚀 Paso 1: Obtener tu IP Real de Windows

### **Opción A: Comando CMD**
```bash
# Abrir CMD como Administrador y ejecutar:
ipconfig

```

### **Opción B: PowerShell**
```bash
# Abrir PowerShell como Administrador y ejecutar:
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"}
```

### **Opción C: Interfaz Gráfica**
1. Abrir **Configuración de Windows**
2. Ir a **Red e Internet** > **Estado**
3. Hacer clic en **Propiedades** de tu conexión activa
4. Buscar **Dirección IPv4**

### **¿Cuál IP Usar?**
Busca una IP que comience con:
- `192.168.x.x` (Red doméstica/oficina)
- `10.x.x.x` (Red empresarial)
- `172.16.x.x` a `172.31.x.x` (Red privada)

**Ejemplo:** Si tu IP es `192.168.1.100`, esa será tu **TU_IP_REAL**.

---

## 🔧 Paso 2: Configurar Firewall de Windows

### **Abrir Puertos Necesarios**

```bash
# Abrir CMD como Administrador y ejecutar:

# Permitir puerto 3000 (Frontend)
netsh advfirewall firewall add rule name="ModelSecurity Frontend" dir=in action=allow protocol=TCP localport=3000

# Permitir puerto 5000 (API)
netsh advfirewall firewall add rule name="ModelSecurity API" dir=in action=allow protocol=TCP localport=5000

# Verificar que se agregaron las reglas
netsh advfirewall firewall show rule name="ModelSecurity Frontend"
netsh advfirewall firewall show rule name="ModelSecurity API"
```

### **Verificar Estado del Firewall**
```bash
# Ver estado general del firewall
netsh advfirewall show allprofiles state

# Listar todas las reglas de entrada
netsh advfirewall firewall show rule dir=in name=all | findstr "ModelSecurity"
```

---

## ⚙️ Paso 3: Configurar la Aplicación para Acceso Externo

### **3.1. Entrar al Contenedor Ubuntu**
```bash
# Desde Windows, entrar al contenedor
docker exec -it modelo-security-app bash
```

### **3.2. Detener Servicios Actuales**
```bash
# Detener API actual
pkill dotnet

# Verificar que se detuvo
ps aux | grep dotnet
```

### **3.3. Actualizar Configuración de la API**

**⚠️ IMPORTANTE:** Reemplaza `TU_IP_REAL` con tu IP real (ejemplo: `192.168.1.100`)

```bash
# Crear nueva configuración con tu IP real
cat > /opt/modelsecurity-api/appsettings.Production.json << 'EOF'
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=ModelSecurity;User=root;Password=1234567;AllowPublicKeyRetrieval=true;",
    "PostgresDb": "Host=localhost;Port=5432;Database=ModelSecurity;Username=postgres;Password=1234567;"
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
    "http://TU_IP_REAL:3000",
    "https://TU_IP_REAL:3000",
    "http://localhost:3000",
    "https://localhost:3000"
  ]
}
EOF
```

### **3.4. Actualizar Configuración del Frontend**

```bash
# Actualizar configuración JavaScript del frontend
cat > /var/www/html/js/config.js << 'EOF'
const CONFIG = {
    getApiUrl: function() {
        // Detectar si se accede desde la IP real o localhost
        const hostname = window.location.hostname;
        
        if (hostname === 'localhost' || hostname === '127.0.0.1') {
            return 'http://TU_IP_REAL:5000/api';
        } else {
            // Para acceso externo, usar la misma IP que el frontend
            return `http://${hostname}:5000/api`;
        }
    }
};

window.API_BASE_URL = CONFIG.getApiUrl();
EOF
```

### **3.5. Actualizar Configuración de Nginx**

```bash
# Configurar Nginx para aceptar conexiones externas
cat > /etc/nginx/sites-available/default << 'EOF'
server {
    listen 3000;
    server_name TU_IP_REAL localhost _;
    root /var/www/html;
    index index.html;

    # Permitir conexiones desde cualquier origen
    location / {
        try_files $uri $uri/ /index.html;
        
        # Headers para CORS si es necesario
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range' always;
    }

    # Optimización para archivos estáticos
    location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF
```

### **3.6. Reiniciar Todos los Servicios**

```bash
# Reiniciar Nginx
service nginx restart

# Verificar configuración de Nginx
nginx -t

# Reiniciar API
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &

# Verificar que está corriendo
ps aux | grep dotnet
```

---

## 🧪 Paso 4: Probar el Acceso Externo

### **4.1. Pruebas desde tu PC (Localhost)**
```bash
# Verificar que sigue funcionando localmente
# Abrir navegador en:
http://localhost:3000
http://localhost:5000
```

### **4.2. Pruebas desde Otra Máquina en la Red**

**Desde cualquier PC en la misma red:**

1. **Abrir navegador web**
2. **Ir a las siguientes URLs** (reemplaza con tu IP real):
   ```
   Frontend: http://192.168.1.100:3000
   API: http://192.168.1.100:5000
   ```

### **4.3. Pruebas con cURL (Opcional)**

```bash
# Desde otra máquina, probar API:
curl http://192.168.1.100:5000/api/health

# Probar frontend:
curl -I http://192.168.1.100:3000
```

---

## 🔍 Paso 5: Verificación y Troubleshooting

### **5.1. Verificar que los Puertos Están Abiertos**

**Desde tu PC con la aplicación:**
```bash
# Verificar puertos abiertos
netstat -an | findstr ":3000"
netstat -an | findstr ":5000"
```

**Desde otra máquina en la red:**
```bash
# Probar conectividad (sustituir IP)
telnet 192.168.1.100 3000
telnet 192.168.1.100 5000
```

### **5.2. Verificar Logs de la Aplicación**

```bash
# Entrar al contenedor
docker exec -it modelo-security-app bash

# Ver logs de API
tail -f /var/log/api.log

# Ver logs de Nginx
tail -f /var/log/nginx/error.log
tail -f /var/log/nginx/access.log
```

### **5.3. Verificar Configuración de Red del Contenedor**

```bash
# Ver configuración de red del contenedor
docker inspect modelo-security-app | findstr IPAddress
docker port modelo-security-app
```

---

## ⚠️ Problemas Comunes y Soluciones

### **Error: "No se puede conectar"**

**Causa:** Firewall bloqueando puertos
**Solución:**
```bash
# Verificar reglas de firewall
netsh advfirewall firewall show rule name="ModelSecurity Frontend"

# Si no existen, crearlas:
netsh advfirewall firewall add rule name="ModelSecurity Frontend" dir=in action=allow protocol=TCP localport=3000
netsh advfirewall firewall add rule name="ModelSecurity API" dir=in action=allow protocol=TCP localport=5000
```

### **Error: "CORS Policy"**

**Causa:** Configuración incorrecta de CORS
**Solución:**
```bash
# Verificar configuración
cat /opt/modelsecurity-api/appsettings.Production.json

# Debe incluir tu IP en "OrigenesPermitidos"
```

### **Error: "API no responde desde otra máquina"**

**Causa:** API escuchando solo en localhost
**Solución:**
```bash
# Verificar que API escuche en 0.0.0.0:5000
ps aux | grep dotnet
netstat -tulpn | grep :5000

# Debe mostrar: 0.0.0.0:5000, no 127.0.0.1:5000
```

### **Frontend carga pero no conecta a API**

**Causa:** Configuración incorrecta de config.js
**Solución:**
```bash
# Verificar configuración del frontend
cat /var/www/html/js/config.js

# El archivo debe detectar automáticamente la IP
```

---

## 🔧 Script Automático de Configuración

### **Script para Configuración Rápida**

```bash
#!/bin/bash
# Script: configure-external-access.sh

echo "🌐 Configurando acceso externo para ModelSecurity"

# Solicitar IP real
read -p "📡 Ingresa tu IP real de Windows (ej: 192.168.1.100): " SERVER_IP

if [ -z "$SERVER_IP" ]; then
    echo "❌ IP requerida"
    exit 1
fi

echo "🔧 Configurando para IP: $SERVER_IP"

# Detener API actual
pkill dotnet

# Actualizar configuración de API
cat > /opt/modelsecurity-api/appsettings.Production.json << EOF
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=ModelSecurity;User=root;Password=1234567;AllowPublicKeyRetrieval=true;",
    "PostgresDb": "Host=localhost;Port=5432;Database=ModelSecurity;Username=postgres;Password=1234567;"
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

# Actualizar configuración de frontend
cat > /var/www/html/js/config.js << 'EOF'
const CONFIG = {
    getApiUrl: function() {
        const hostname = window.location.hostname;
        if (hostname === 'localhost' || hostname === '127.0.0.1') {
            return 'http://localhost:5000/api';
        } else {
            return `http://${hostname}:5000/api`;
        }
    }
};
window.API_BASE_URL = CONFIG.getApiUrl();
EOF

# Actualizar Nginx
cat > /etc/nginx/sites-available/default << EOF
server {
    listen 3000;
    server_name $SERVER_IP localhost _;
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

# Reiniciar servicios
service nginx restart
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &

echo ""
echo "✅ Configuración completada"
echo "🌐 Acceso externo disponible en:"
echo "   Frontend: http://$SERVER_IP:3000"
echo "   API: http://$SERVER_IP:5000"
echo ""
echo "⚠️  No olvides configurar el firewall de Windows:"
echo "   netsh advfirewall firewall add rule name=\"ModelSecurity Frontend\" dir=in action=allow protocol=TCP localport=3000"
echo "   netsh advfirewall firewall add rule name=\"ModelSecurity API\" dir=in action=allow protocol=TCP localport=5000"
```

### **Uso del Script:**
```bash
# Crear el script dentro del contenedor
cat > /opt/configure-external-access.sh << 'EOF'
[PEGAR CONTENIDO DEL SCRIPT AQUÍ]
EOF

# Dar permisos y ejecutar
chmod +x /opt/configure-external-access.sh
/opt/configure-external-access.sh
```

---

## 📱 Acceso desde Diferentes Dispositivos

### **Computadoras en la Red**
- Abrir navegador web
- Ir a: `http://TU_IP_REAL:3000`

### **Móviles en la Misma WiFi**
- Abrir navegador del móvil
- Ir a: `http://TU_IP_REAL:3000`

### **Tabletas**
- Mismo procedimiento que móviles
- Funciona en Safari, Chrome, Firefox, etc.

---

## 🔒 Consideraciones de Seguridad

### **Para Entorno de Desarrollo**
- ✅ Configuración actual es adecuada
- ✅ Solo acceso en red local
- ✅ Bases de datos protegidas

### **Para Producción**
```bash
# Cambiar contraseñas por defecto
# Configurar HTTPS con certificados SSL
# Implementar autenticación adicional
# Configurar firewall más restrictivo
```

---

## ✅ Verificación Final

Tu aplicación está correctamente configurada para acceso externo si:

1. ✅ Firewall de Windows permite puertos 3000 y 5000
2. ✅ Frontend carga desde: `http://TU_IP_REAL:3000`
3. ✅ API responde desde: `http://TU_IP_REAL:5000`
4. ✅ Otras máquinas pueden registrar usuarios
5. ✅ No hay errores de CORS
6. ✅ Bases de datos funcionan correctamente

---

## 🆘 Soporte Rápido

**Si algo no funciona:**

1. **Verificar firewall**: `netsh advfirewall firewall show rule name=all | findstr ModelSecurity`
2. **Verificar API**: `docker exec -it modelo-security-app tail -f /var/log/api.log`
3. **Verificar puertos**: `netstat -an | findstr ":3000\|:5000"`
4. **Reiniciar todo**: Ejecutar script de configuración nuevamente

---

**🌐 ¡Ahora tu aplicación ModelSecurity es accesible desde cualquier dispositivo en tu red!**

*Configurado para acceso externo con .NET 8, MySQL, PostgreSQL y Nginx*
# 🔧 ModelSecurity - Gestión y Reinicio de Servicios

Guía completa para **gestionar**, **reiniciar** y **mantener** tu aplicación ModelSecurity después del despliegue inicial.

## 📋 Información Importante

⚠️ **Al reiniciar el contenedor, los servicios NO se inician automáticamente**

Cuando detienes y vuelves a iniciar el contenedor Docker, necesitas **reiniciar manualmente** todos los servicios:
- MySQL
- PostgreSQL  
- Nginx
- API .NET

---

## 🔄 Gestión Básica del Contenedor

### **Detener Contenedor**
```bash
# Desde Windows (CMD o PowerShell)
docker stop modelo-security-app
```

### **Iniciar Contenedor**
```bash
# Desde Windows  
docker start modelo-security-app
```

### **Ver Estado del Contenedor**
```bash
# Ver todos los contenedores
docker ps -a

# Ver solo contenedores activos
docker ps
```

### **Entrar al Contenedor**
```bash
# Acceso interactivo
docker exec -it modelo-security-app bash
```

---

## 🚀 Reinicio de Servicios (Método Rápido)

### **Opción 1: Script Automático (Recomendado)**

```bash
# Desde Windows - Reinicio completo en un comando
docker exec -it modelo-security-app /opt/start-services.sh
```

### **Opción 2: Paso a Paso Manual**

```bash
# 1. Entrar al contenedor
docker exec -it modelo-security-app bash

# 2. Dentro del contenedor, ejecutar:
/opt/start-services.sh
```

### **Si el script no existe, crearlo:**

```bash
# Entrar al contenedor
docker exec -it modelo-security-app bash

# Crear script de inicio
cat > /opt/start-services.sh << 'EOF'
#!/bin/bash

echo "🚀 Iniciando servicios ModelSecurity..."

# Iniciar MySQL
service mysql start
echo "✅ MySQL iniciado en puerto 3306"

# Iniciar PostgreSQL  
service postgresql start
echo "✅ PostgreSQL iniciado en puerto 5432"

# Iniciar Nginx
service nginx start
echo "✅ Nginx iniciado en puerto 3000"

# Esperar que las bases de datos estén listas
sleep 5

# Iniciar API
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &
echo "✅ API iniciada en puerto 5000"

echo ""
echo "🌐 Aplicación disponible en:"
echo "   Frontend: http://localhost:3000"
echo "   API: http://localhost:5000"
echo "   Frontend Externo: http://192.168.101.14:3000"
echo "   API Externa: http://192.168.101.14:5000"
echo ""
echo "📊 Para ver logs de la API: tail -f /var/log/api.log"
EOF

# Dar permisos de ejecución
chmod +x /opt/start-services.sh

# Ejecutar el script
/opt/start-services.sh
```

---

## 🔍 Verificación de Estado de Servicios

### **Verificar Todos los Servicios**
```bash
# Entrar al contenedor
docker exec -it modelo-security-app bash

# Ver procesos activos
ps aux | grep -E "(nginx|mysql|postgres|dotnet)"

# Ver puertos en uso
netstat -tulpn | grep -E "(3000|5000|3306|5432)"
```

### **Verificar Servicios Individualmente**

```bash
# Estado de MySQL
service mysql status

# Estado de PostgreSQL
service postgresql status

# Estado de Nginx
service nginx status

# Ver proceso de la API
ps aux | grep dotnet
```

### **Verificar Conectividad**

```bash
# Probar MySQL
mysql -u root -p1234567 -e "SELECT 1;"

# Probar PostgreSQL
su - postgres -c "psql -d ModelSecurity -c 'SELECT 1;'"

# Probar API (desde otro terminal)
curl http://localhost:5000
curl http://192.168.101.14:5000
```

---

## 🛠️ Gestión de Servicios Individuales

### **MySQL**
```bash
# Iniciar
service mysql start

# Detener  
service mysql stop

# Reiniciar
service mysql restart

# Ver estado
service mysql status
```

### **PostgreSQL**
```bash
# Iniciar
service postgresql start

# Detener
service postgresql stop

# Reiniciar  
service postgresql restart

# Ver estado
service postgresql status
```

### **Nginx**
```bash
# Iniciar
service nginx start

# Detener
service nginx stop

# Reiniciar
service nginx restart

# Ver estado
service nginx status

# Verificar configuración
nginx -t
```

### **API (.NET)**
```bash
# Detener API actual
pkill dotnet

# Iniciar API
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &

# Ver logs en tiempo real
tail -f /var/log/api.log
```

---

## 📊 Monitoreo y Logs

### **Ver Logs de la API**
```bash
# Ver logs en tiempo real
docker exec -it modelo-security-app tail -f /var/log/api.log

# Ver últimas 50 líneas
docker exec -it modelo-security-app tail -n 50 /var/log/api.log

# Buscar errores
docker exec -it modelo-security-app grep -i error /var/log/api.log
```

### **Ver Logs de Nginx**
```bash
# Logs de acceso
docker exec -it modelo-security-app tail -f /var/log/nginx/access.log

# Logs de errores
docker exec -it modelo-security-app tail -f /var/log/nginx/error.log
```

### **Ver Logs de MySQL**
```bash
# Log general (ubicación puede variar)
docker exec -it modelo-security-app tail -f /var/log/mysql/error.log
```

### **Ver Logs de PostgreSQL**
```bash
# Logs de PostgreSQL
docker exec -it modelo-security-app tail -f /var/log/postgresql/postgresql-*-main.log
```

---

## 🚨 Solución de Problemas Comunes

### **"El contenedor no inicia"**
```bash
# Ver logs del contenedor
docker logs modelo-security-app

# Iniciar en modo interactivo para diagnosticar
docker run -it --rm -p 3000:3000 -p 5000:5000 ubuntu:latest bash
```

### **"Los servicios no inician"**
```bash
# Verificar espacio en disco
df -h

# Verificar memoria
free -h

# Verificar permisos
ls -la /opt/start-services.sh
chmod +x /opt/start-services.sh
```

### **"MySQL no conecta"**
```bash
# Verificar proceso
ps aux | grep mysql

# Iniciar manualmente
service mysql start

# Ver logs de error
tail -f /var/log/mysql/error.log

# Reiniciar servicio
service mysql restart
```

### **"PostgreSQL no conecta"**  
```bash
# Verificar proceso
ps aux | grep postgres

# Iniciar manualmente
service postgresql start

# Verificar configuración
su - postgres -c "psql -c '\\l'"
```

### **"Nginx no sirve el frontend"**
```bash
# Verificar configuración
nginx -t

# Ver archivos del frontend
ls -la /var/www/html/

# Verificar permisos
chmod -R 755 /var/www/html/

# Reiniciar nginx
service nginx restart
```

### **"API devuelve Error 500"**
```bash
# Ver logs detallados
tail -f /var/log/api.log

# Verificar configuración
cat /opt/modelsecurity-api/appsettings.Production.json

# Verificar bases de datos
mysql -u root -p1234567 -e "SELECT 1;"
su - postgres -c "psql -d ModelSecurity -c 'SELECT 1;'"

# Reiniciar API
pkill dotnet
cd /opt/modelsecurity-api && nohup dotnet Web.dll > /var/log/api.log 2>&1 &
```

---

## ⚡ Scripts de Automatización

### **Script de Reinicio Completo**
```bash
# Crear script avanzado
cat > /opt/full-restart.sh << 'EOF'
#!/bin/bash
echo "🔄 Reinicio completo de ModelSecurity..."

# Detener servicios
echo "🛑 Deteniendo servicios..."
pkill dotnet 2>/dev/null
service nginx stop 2>/dev/null
service postgresql stop 2>/dev/null  
service mysql stop 2>/dev/null

sleep 3

# Iniciar servicios
echo "🚀 Iniciando servicios..."
service mysql start
service postgresql start
service nginx start

# Esperar que las bases de datos estén listas
sleep 5

# Iniciar API
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &

echo "✅ Reinicio completado"
echo "🌐 Frontend: http://localhost:3000 | http://192.168.101.14:3000"
echo "🔧 API: http://localhost:5000 | http://192.168.101.14:5000"
EOF

chmod +x /opt/full-restart.sh
```

### **Script de Verificación de Estado**
```bash
# Crear script de diagnóstico
cat > /opt/check-status.sh << 'EOF'
#!/bin/bash
echo "📊 Estado de ModelSecurity"
echo "========================="

# Verificar servicios
echo "🔍 Servicios activos:"
ps aux | grep -E "(nginx|mysql|postgres|dotnet)" | grep -v grep

echo ""
echo "🌐 Puertos en uso:"
netstat -tulpn | grep -E "(3000|5000|3306|5432)"

echo ""
echo "💾 Conectividad de bases de datos:"
mysql -u root -p1234567 -e "SELECT 'MySQL OK' as Status;" 2>/dev/null || echo "❌ MySQL desconectado"
su - postgres -c "psql -d ModelSecurity -c \"SELECT 'PostgreSQL OK' as Status;\"" 2>/dev/null || echo "❌ PostgreSQL desconectado"

echo ""
echo "📁 Archivos del frontend:"
ls -la /var/www/html/index.html 2>/dev/null && echo "✅ Frontend OK" || echo "❌ Frontend faltante"

echo ""
echo "🔧 API:"
curl -s http://localhost:5000 >/dev/null && echo "✅ API respondiendo" || echo "❌ API no responde"
EOF

chmod +x /opt/check-status.sh
```

### **Usar los Scripts**
```bash
# Desde Windows
docker exec -it modelo-security-app /opt/full-restart.sh
docker exec -it modelo-security-app /opt/check-status.sh
```

---

## 🔄 Inicio Automático (Opcional)

### **Configurar Inicio Automático al Entrar al Contenedor**
```bash
# Agregar script al perfil de bash
echo '# Auto-start ModelSecurity services' >> /root/.bashrc
echo 'if [ ! -f /tmp/services-started ]; then' >> /root/.bashrc  
echo '  echo "🚀 Iniciando servicios ModelSecurity..."' >> /root/.bashrc
echo '  /opt/start-services.sh' >> /root/.bashrc
echo '  touch /tmp/services-started' >> /root/.bashrc
echo 'fi' >> /root/.bashrc
```

### **Contenedor con Política de Reinicio Automático**
```bash
# Detener y eliminar contenedor actual
docker stop modelo-security-app
docker rm modelo-security-app

# Crear nuevo contenedor con restart automático
docker run -it -d --name modelo-security-app --restart=unless-stopped -p 3000:3000 -p 5000:5000 ubuntu:latest bash

# Luego seguir el proceso de instalación inicial
```

---

## 📋 Comandos de Referencia Rápida

### **Desde Windows (Un Solo Comando)**
```bash
# Reiniciar contenedor y servicios
docker start modelo-security-app && docker exec -it modelo-security-app /opt/start-services.sh

# Ver estado completo  
docker exec -it modelo-security-app /opt/check-status.sh

# Ver logs de API
docker exec -it modelo-security-app tail -f /var/log/api.log

# Entrar al contenedor
docker exec -it modelo-security-app bash
```

### **Dentro del Contenedor**
```bash
# Iniciar todos los servicios
/opt/start-services.sh

# Reiniciar completamente
/opt/full-restart.sh

# Verificar estado
/opt/check-status.sh

# Ver logs de API
tail -f /var/log/api.log
```

---

## ✅ Verificación Final

Tu aplicación está correctamente funcionando si:

1. ✅ **Contenedor iniciado**: `docker ps` muestra `modelo-security-app`
2. ✅ **Servicios corriendo**: `/opt/check-status.sh` muestra todos los servicios activos
3. ✅ **Frontend accesible**: http://localhost:3000 y http://192.168.101.14:3000
4. ✅ **API accesible**: http://localhost:5000 y http://192.168.101.14:5000
5. ✅ **Bases de datos conectadas**: MySQL y PostgreSQL responden
6. ✅ **Sin errores en logs**: `tail -f /var/log/api.log` no muestra errores

---

## 🎯 Rutina de Reinicio Típica

**Cada vez que reinicies el contenedor:**

1. `docker start modelo-security-app`
2. `docker exec -it modelo-security-app /opt/start-services.sh`
3. Verificar en navegador: http://localhost:3000

**¡Listo! Tu aplicación estará funcionando nuevamente.**

---

**🔧 ¡Tu aplicación ModelSecurity está completamente gestionada y lista para usar!**

*Guía completa para reinicio, monitoreo y mantenimiento de servicios*
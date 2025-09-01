# 🚀 ModelSecurity - Despliegue en Contenedor Ubuntu

Aplicación completa con backend .NET y frontend que se conecta a **MySQL** y **PostgreSQL** simultáneamente.

## ⚡ Despliegue Rápido

```bash
# 1. Clonar repositorio
git clone https://github.com/JuanCarlosJC-20/ModelSecurity.git
cd ModelSecurity

# 2. Ejecutar script (como root)
chmod +x deploy-container.sh
./deploy-container.sh

# 3. Iniciar servicios
/opt/start-services.sh
```

## 🎯 ¿Qué instala el script?

- ✅ **.NET 9 SDK** - Runtime para la API
- ✅ **MySQL** - Base de datos principal
- ✅ **PostgreSQL** - Base de datos secundaria  
- ✅ **SQL Server** - Base de datos adicional
- ✅ **Nginx** - Servidor web para frontend
- ✅ **Configuración automática** de todas las conexiones

## 🌐 URLs de Acceso

Una vez completado el despliegue:

- **🌐 Frontend**: `http://TU_IP:3000`
- **🔧 API**: `http://TU_IP:5000`

## 🗄️ Bases de Datos Disponibles

| Base de Datos | Puerto | Usuario | Contraseña | 
|---------------|--------|---------|------------|
| MySQL         | 3306   | root    | 1234567    |
| PostgreSQL    | 5432   | postgres| 1234567    |

## 🔧 Comandos Útiles

### **Gestión de Servicios**
```bash
# Iniciar todos los servicios:
/opt/start-services.sh

# Ver logs de la API:
tail -f /var/log/api.log

# Verificar procesos:
ps aux | grep -E "(nginx|mysql|postgres|sqlservr|dotnet)"
```

### **Verificar Bases de Datos**
```bash
# MySQL
mysql -u root -p1234567 -e 'SHOW DATABASES;'

# PostgreSQL  
sudo -u postgres psql -c '\l'

# SQL Server
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'SqlServer2025!' -C -Q 'SELECT name FROM sys.databases;'
```

### **Reiniciar Servicios Individuales**
```bash
# Reiniciar MySQL
service mysql restart

# Reiniciar PostgreSQL
service postgresql restart

# Reiniciar SQL Server
service mssql-server restart

# Reiniciar Nginx
service nginx restart

# Reiniciar API (matar proceso y ejecutar de nuevo)
pkill dotnet
cd /opt/modelsecurity-api && nohup dotnet Web.dll > /var/log/api.log 2>&1 &
```

## 📊 Características de la Aplicación

### **Backend (.NET 9)**
- ✅ Conecta a **3 bases de datos simultáneamente**
- ✅ **Entity Framework Core** para cada base de datos
- ✅ **Migraciones automáticas** en todas las bases
- ✅ **JWT Authentication**
- ✅ **CORS configurado** para el frontend
- ✅ **API RESTful** completa

### **Frontend (HTML/CSS/JS)**
- ✅ **Detección automática** de la URL del API
- ✅ **Interfaz responsiva**
- ✅ **Autenticación JWT**
- ✅ **Dashboard completo**
- ✅ Servido por **Nginx**

## 🔒 Configuración de Seguridad

### **Para Producción**
1. **Cambiar contraseñas** por defecto
2. **Configurar HTTPS** con certificados SSL
3. **Configurar firewall** para los puertos necesarios
4. **Actualizar JWT secret** en appsettings

### **Puertos a Abrir**
```bash
# Si tienes firewall, abre estos puertos:
ufw allow 3000  # Frontend
ufw allow 5000  # API
ufw allow 3306  # MySQL  
ufw allow 5432  # PostgreSQL
ufw allow 1433  # SQL Server
```

## 🗂️ Estructura del Proyecto

```
ModelSecurity/
├── Backend/
│   ├── Business/     # Lógica de negocio
│   ├── Data/         # Acceso a datos
│   ├── Entity/       # Entidades
│   ├── Web/          # API Controllers
│   └── Utilities/    # Utilidades
├── frontend/
│   ├── js/           # JavaScript
│   ├── styles/       # CSS
│   └── *.html        # Páginas
└── deploy-container.sh  # Script de despliegue
```

## 🐛 Solución de Problemas

### **API no inicia**
```bash
# Ver logs detallados
tail -f /var/log/api.log

# Verificar configuración
cat /opt/modelsecurity-api/appsettings.Production.json

# Verificar puertos
netstat -tulpn | grep -E "(5000|3000|3306|5432|1433)"
```

### **Bases de datos no conectan**
```bash
# Verificar servicios
service mysql status
service postgresql status  
service mssql-server status

# Probar conexiones manualmente
mysql -u root -p1234567 -e "SELECT 1;"
sudo -u postgres psql -c "SELECT 1;"
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'SqlServer2025!' -C -Q "SELECT 1;"
```

### **Frontend no carga**
```bash
# Verificar archivos
ls -la /var/www/html/

# Verificar Nginx
service nginx status
nginx -t

# Ver logs de Nginx
tail -f /var/log/nginx/error.log
```

## 📝 Notas Importantes

- ⚠️ **Ejecutar como root** en contenedor Ubuntu
- ⚠️ **No usar en producción** sin cambiar credenciales
- ⚠️ **Tiempo de instalación**: 10-15 minutos aprox
- ✅ **Compatible con Ubuntu 20.04+**
- ✅ **Tu API guarda información en las 3 bases de datos**
- ✅ **Reinicia automáticamente** si el contenedor se reinicia

## 🆘 Soporte

Si tienes problemas:

1. **Ver logs**: `tail -f /var/log/api.log`
2. **Verificar servicios**: `ps aux | grep -E "(mysql|postgres|sqlservr|nginx|dotnet)"`
3. **Reiniciar todo**: `/opt/start-services.sh`
4. **Verificar puertos**: `netstat -tulpn | grep -E "(3000|5000|3306|5432|1433)"`

---

**🚀 ¡Tu aplicación ModelSecurity está lista para usar con las 3 bases de datos!**
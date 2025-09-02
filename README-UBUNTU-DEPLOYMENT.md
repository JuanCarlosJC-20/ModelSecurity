# 🚀 ModelSecurity - Despliegue en Ubuntu Container

Guía completa para desplegar la aplicación **ModelSecurity** con backend .NET 8 y frontend en un contenedor Ubuntu.

## 📋 Requisitos Previos

- Docker instalado en Windows
- Git para clonar el repositorio
- Puerto 3000 y 5000 disponibles

## 🎯 Arquitectura de la Aplicación

### **Backend (.NET 8)**
- ✅ API RESTful con Entity Framework Core
- ✅ Conecta a **MySQL** y **PostgreSQL** simultáneamente
- ✅ JWT Authentication
- ✅ Swagger UI integrado
- ✅ Migraciones automáticas

### **Frontend (HTML/CSS/JS)**
- ✅ Interfaz web responsiva
- ✅ Servido por **Nginx** en puerto 3000
- ✅ Conecta automáticamente a la API

### **Bases de Datos**
- 🔵 **MySQL**: Base de datos principal (puerto 3306)
- 🟢 **PostgreSQL**: Base de datos secundaria (puerto 5432)

---

## 🚀 Despliegue Paso a Paso

### **1. Preparar el Contenedor Ubuntu**

```bash
# Crear y ejecutar contenedor Ubuntu con puertos mapeados
docker run -it --name modelo-security-app -p 3000:3000 -p 5000:5000 ubuntu:latest bash

# Verificar que estás dentro del contenedor
whoami  # Debe mostrar: root
```

### **2. Clonar el Repositorio**

```bash
# Actualizar sistema e instalar Git
apt update && apt install -y git

# Clonar el repositorio
git clone https://github.com/JuanCarlosJC-20/ModelSecurity.git
cd ModelSecurity
```

### **3. Ejecutar Script de Instalación**

```bash
# Dar permisos al script
chmod +x deploy-container.sh

# Ejecutar instalación (toma 10-15 minutos)
./deploy-container.sh

# Cuando pida IP del servidor, usar: localhost
# Presiona Enter para usar la IP por defecto
```

### **4. Configurar Permisos de MySQL**

```bash
# Conectar a MySQL
mysql -u root

# Dentro de MySQL, ejecutar:
```

```sql
ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY '1234567';
GRANT ALL PRIVILEGES ON *.* TO 'root'@'localhost' WITH GRANT OPTION;
FLUSH PRIVILEGES;
EXIT;
```

```bash
# Verificar conexión
mysql -u root -p1234567 -e "SELECT 1;"
```

### **5. Crear Script de Inicio**

```bash
# Crear script de servicios
cat > /opt/start-services.sh << 'EOF'
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
echo "✅ Nginx iniciado en puerto 3000"

# Iniciar API en background
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
nohup dotnet Web.dll > /var/log/api.log 2>&1 &
echo "✅ API iniciada en puerto 5000"

echo ""
echo "🌐 Aplicación disponible en:"
echo "   Frontend: http://localhost:3000"
echo "   API: http://localhost:5000"
echo ""
echo "🗄️ Bases de datos disponibles:"
echo "   MySQL: localhost:3306 (root/1234567)"
echo "   PostgreSQL: localhost:5432 (postgres/1234567)"
echo ""
echo "📊 Para ver logs de la API: tail -f /var/log/api.log"

# Mostrar logs de la API
tail -f /var/log/api.log
EOF

# Dar permisos
chmod +x /opt/start-services.sh
```

### **6. Iniciar Todos los Servicios**

```bash
# Ejecutar script de inicio
/opt/start-services.sh
```

---

## 🌐 Acceso a la Aplicación

Una vez completado el despliegue:

- **🌐 Frontend**: http://localhost:3000
- **🔧 API Swagger**: http://localhost:5000
- **📊 Endpoints API**: http://localhost:5000/api/

## 🗄️ Información de Bases de Datos

| Base de Datos | Puerto | Usuario   | Contraseña | Uso                    |
|---------------|--------|-----------|------------|------------------------|
| MySQL         | 3306   | root      | 1234567    | Base de datos principal |
| PostgreSQL    | 5432   | postgres  | 1234567    | Base de datos secundaria |

### **Cómo Funcionan las Conexiones**

1. **Entity Framework Core** maneja dos contextos de base de datos:
   - `MySqlDbContext`: Para operaciones en MySQL
   - `PostgresDbContext`: Para operaciones en PostgreSQL

2. **MultiDatabaseService** distribuye las operaciones:
   - Ambas bases de datos mantienen el mismo esquema
   - Los datos se replican automáticamente
   - Si una falla, la otra continúa funcionando

3. **Migraciones Automáticas**:
   - Al iniciar, la API ejecuta migraciones en ambas bases
   - Crea tablas y datos iniciales si no existen
   - Se ejecuta seeding para datos por defecto

---

## 🔧 Comandos Útiles

### **Gestión de Contenedor**

```bash
# Entrar al contenedor (desde Windows)
docker exec -it modelo-security-app bash

# Ver contenedores activos
docker ps

# Detener/iniciar contenedor
docker stop modelo-security-app
docker start modelo-security-app
```

### **Gestión de Servicios**

```bash
# Verificar servicios activos
ps aux | grep -E "(nginx|mysql|postgres|dotnet)"

# Verificar puertos
netstat -tulpn | grep -E "(3000|5000|3306|5432)"

# Ver logs de la API
tail -f /var/log/api.log

# Reiniciar servicios individuales
service mysql restart
service postgresql restart
service nginx restart
```

### **Gestión de API**

```bash
# Matar proceso API
pkill dotnet

# Reiniciar API manualmente
cd /opt/modelsecurity-api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
dotnet Web.dll

# Reiniciar API en background
nohup dotnet Web.dll > /var/log/api.log 2>&1 &
```

### **Verificar Bases de Datos**

```bash
# MySQL
mysql -u root -p1234567 -e "SHOW DATABASES; USE ModelSecurity; SHOW TABLES;"

# PostgreSQL
su - postgres -c "psql -c '\l'"
su - postgres -c "psql -d ModelSecurity -c '\dt'"
```

---

## 🐛 Solución de Problemas

### **Frontend no carga (Error 404)**
```bash
# Verificar Nginx
service nginx status
nginx -t

# Ver logs de Nginx
tail -f /var/log/nginx/error.log

# Reiniciar Nginx
service nginx restart
```

### **API devuelve Error 500**
```bash
# Ver logs detallados
tail -f /var/log/api.log

# Verificar configuración
cat /opt/modelsecurity-api/appsettings.Production.json

# Probar conexión a bases de datos
mysql -u root -p1234567 -e "SELECT 1;"
```

### **Error de CORS**
El error se resuelve automáticamente al iniciar correctamente la API. La configuración CORS permite conexiones desde `http://localhost:3000`.

### **Bases de datos no conectan**
```bash
# Verificar servicios
service mysql status
service postgresql status

# Reiniciar bases de datos
service mysql restart
service postgresql restart

# Verificar permisos MySQL
mysql -u root -p1234567 -e "SELECT User, Host FROM mysql.user WHERE User='root';"
```

---

## 🔄 Reinicio Después de Reiniciar Contenedor

Si reinicias el contenedor, solo ejecuta:

```bash
# Entrar al contenedor
docker exec -it modelo-security-app bash

# Ejecutar script de inicio
/opt/start-services.sh
```

---

## 🔒 Configuración de Seguridad

### **Para Producción (Recomendado)**

1. **Cambiar contraseñas por defecto**
2. **Configurar HTTPS** con certificados SSL
3. **Actualizar JWT secret** en appsettings
4. **Configurar firewall** para puertos específicos

### **Puertos a Abrir en Firewall**
```bash
# Si usas firewall, abre estos puertos:
ufw allow 3000  # Frontend
ufw allow 5000  # API
```

---

## 📊 Funcionalidades de la API

### **Endpoints Principales**
- `POST /api/Auth/register` - Registro de usuarios
- `POST /api/Auth/login` - Login de usuarios
- `GET /api/Auth/profile` - Perfil del usuario
- `GET /api/User` - Gestión de usuarios
- `GET /api/Role` - Gestión de roles
- `GET /api/Permission` - Gestión de permisos

### **Autenticación**
- Sistema basado en **JWT tokens**
- Headers requeridos: `Authorization: Bearer <token>`
- Tokens expiran en 24 horas por defecto

---

## ✅ Verificación Final

La aplicación está correctamente configurada si:

1. ✅ **Frontend** carga en http://localhost:3000
2. ✅ **Swagger UI** carga en http://localhost:5000
3. ✅ **Registro de usuarios** funciona sin errores CORS
4. ✅ **Bases de datos** responden a consultas
5. ✅ **API endpoints** devuelven respuestas válidas

---

## 🆘 Soporte

Si encuentras problemas:

1. **Verificar logs**: `tail -f /var/log/api.log`
2. **Verificar servicios**: `ps aux | grep -E "(nginx|mysql|postgres|dotnet)"`
3. **Reiniciar todo**: `/opt/start-services.sh`
4. **Verificar puertos**: `netstat -tulpn | grep -E "(3000|5000|3306|5432)"`

---

**🚀 ¡Tu aplicación ModelSecurity está lista para usar con doble base de datos!**

*Configurado para .NET 8, MySQL, PostgreSQL y Nginx en Ubuntu Container*
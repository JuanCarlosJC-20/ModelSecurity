# 🚀 Guía de Despliegue en Ubuntu

Esta guía te ayudará a desplegar tu aplicación ModelSecurity en una máquina Ubuntu usando Docker.

## 📋 Requisitos Previos

- Ubuntu 20.04 o superior
- Acceso sudo en el servidor
- Conexión a internet
- Puertos 3000, 5000 y 3306 disponibles

## 🛠️ Instalación Automática

### Opción 1: Script Automático (Recomendado)

```bash
# Clonar el repositorio
git clone https://github.com/tu-usuario/ModelSecurity.git
cd ModelSecurity

# Dar permisos de ejecución al script
chmod +x deploy-ubuntu.sh

# Ejecutar el script de despliegue
./deploy-ubuntu.sh
```

El script automáticamente:
- ✅ Instala Docker y Docker Compose si no están instalados
- ✅ Configura las variables de entorno
- ✅ Construye las imágenes Docker
- ✅ Levanta todos los servicios
- ✅ Verifica que todo funcione correctamente

### Opción 2: Instalación Manual

1. **Instalar Docker y Docker Compose:**
```bash
sudo apt update
sudo apt install -y docker.io docker-compose
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER
# Reiniciar sesión o ejecutar: newgrp docker
```

2. **Clonar y configurar el proyecto:**
```bash
git clone https://github.com/tu-usuario/ModelSecurity.git
cd ModelSecurity/Backend
```

3. **Editar el archivo .env:**
```bash
# Cambiar localhost por la IP de tu servidor Ubuntu
nano .env
```

4. **Levantar los servicios:**
```bash
docker-compose up -d
```

## 🌐 Acceso a la Aplicación

Una vez desplegado, podrás acceder a:

- **Frontend**: http://TU_IP_UBUNTU:3000
- **API**: http://TU_IP_UBUNTU:5000
- **MySQL**: TU_IP_UBUNTU:3306

## 🔧 Comandos Útiles

```bash
# Ver estado de los contenedores
docker-compose ps

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f modelsecurity-api

# Reiniciar todos los servicios
docker-compose restart

# Detener todos los servicios
docker-compose down

# Reconstruir e iniciar (si haces cambios)
docker-compose up -d --build
```

## 🔒 Configuración de Firewall

Si usas ufw (Ubuntu Firewall), permite los puertos necesarios:

```bash
sudo ufw allow 3000
sudo ufw allow 5000
sudo ufw allow 3306
sudo ufw reload
```

## 🐛 Solución de Problemas

### Problema: Error de permisos con Docker
```bash
sudo usermod -aG docker $USER
newgrep docker
# O reiniciar la sesión
```

### Problema: Puerto ya en uso
```bash
# Ver qué está usando el puerto
sudo netstat -tulpn | grep :3000

# Detener servicios anteriores
docker-compose down
```

### Problema: Base de datos no conecta
```bash
# Verificar logs de MySQL
docker-compose logs mysql

# Reiniciar solo MySQL
docker-compose restart mysql
```

### Problema: Frontend no puede conectar con API
1. Verifica que la IP en el archivo .env sea correcta
2. Verifica que el firewall permita el puerto 5000
3. Revisa los logs del API: `docker-compose logs modelsecurity-api`

## 📊 Monitoreo

Para monitorear el estado de tu aplicación:

```bash
# Recursos del sistema
docker stats

# Espacio en disco usado por Docker
docker system df

# Limpiar recursos no utilizados
docker system prune -f
```

## 🔄 Actualizaciones

Para actualizar tu aplicación:

```bash
# Obtener cambios del repositorio
git pull

# Reconstruir y reiniciar
docker-compose down
docker-compose up -d --build
```

## 📝 Notas Importantes

- La base de datos se configura automáticamente en el primer inicio
- Los datos se persisten en volúmenes Docker
- Para producción, cambia las contraseñas por defecto
- Considera usar HTTPS con un proxy reverso (nginx) para producción

## 🆘 Soporte

Si tienes problemas:

1. Revisa los logs: `docker-compose logs`
2. Verifica el estado: `docker-compose ps`
3. Asegúrate de que todos los puertos estén disponibles
4. Verifica la configuración de red y firewall
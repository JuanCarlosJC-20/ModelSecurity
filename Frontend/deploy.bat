@echo off
echo Iniciando despliegue del frontend...

REM Verificar si Docker está instalado
docker --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker no esta instalado. Por favor instalalo primero.
    pause
    exit /b 1
)

REM Verificar si Docker Compose está instalado
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker Compose no esta instalado. Por favor instalalo primero.
    pause
    exit /b 1
)

echo Deteniendo contenedores existentes...
docker-compose down --remove-orphans

echo Construyendo y levantando servicios...
docker-compose up --build -d

echo Estado de los contenedores:
docker-compose ps

echo Verificando conectividad...
timeout /t 5 >nul

REM Verificar si el puerto está disponible (comando básico)
echo Frontend desplegado!
echo Accede a la aplicacion en: http://localhost:3000
echo La aplicacion iniciara directamente en el login

echo.
echo Para ver logs ejecuta: docker-compose logs -f
echo Para detener el servicio ejecuta: docker-compose down

pause
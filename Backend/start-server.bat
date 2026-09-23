@echo off
echo ============================================
echo  SurakshaAR Backend Server
echo ============================================
echo.
echo Your local IP:
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /i "IPv4" ^| findstr /v "127.0.0.1"') do set LOCAL_IP=%%a
set LOCAL_IP=%LOCAL_IP: =%
echo   %LOCAL_IP%:5000
echo.
echo API Base URL: http://%LOCAL_IP%:5000/api/
echo.
echo Starting server... (press Ctrl+C to stop)
echo.
dotnet run --project "%~dp0"

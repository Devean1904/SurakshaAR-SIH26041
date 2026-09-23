@echo off
echo ============================================
echo  SurakshaAR - Open Firewall Port 5000
echo ============================================
echo.
echo This must be run as Administrator.
echo Right-click this file and select "Run as administrator"
echo.
netsh advfirewall firewall add rule name="SurakshaAR Backend" dir=in action=allow protocol=TCP localport=5000
netsh advfirewall firewall add rule name="SurakshaAR Backend UDP" dir=in action=allow protocol=UDP localport=5000
echo.
echo Firewall rules added. Port 5000 is now open.
echo.
pause

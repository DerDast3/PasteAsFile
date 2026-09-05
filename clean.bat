@echo off
echo Removing build artifacts (bin, obj) ...
for /d /r "%~dp0" %%d in (bin obj) do if exist "%%d" rmdir /s /q "%%d"
echo Done. Project is ready for download / publishing.
pause

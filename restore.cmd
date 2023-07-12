@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& '%~dp0eng\common\build.ps1' -nodeReuse:$false -restore %*"
exit /b %ErrorLevel%

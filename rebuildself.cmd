@echo off
setlocal EnableDelayedExpansion
call  "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat"
chcp 65001
msbuild.exe   %~dp0\%1   /t:Rebuild   /p:Configuration=Release;Platform="Any CPU"

@ECHO OFF
SET VERSION=%1
SET CURDIR=%cd%
cd %~dp0

powershell.exe -Command "& {Set-ExecutionPolicy bypass}"
IF %VERSION%.==. GOTO NoParam
  powershell.exe .\solutionsetup.ps1 %VERSION%
GOTO End1

:NoParam
  powershell.exe .\solutionsetup.ps1
GOTO End1

:End1
cd %CURDIR%

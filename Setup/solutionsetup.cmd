@ECHO OFF
SET CURDIR=%cd%
cd %~dp0
powershell.exe -Command "& {Set-ExecutionPolicy bypass}"
powershell.exe .\solutionsetup.ps1
cd %CURDIR%

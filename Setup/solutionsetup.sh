#!/bin/bash

# Powershell wants to run from SETUPDIR
SETUPDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

cd $SETUPDIR

powershell.exe -Command "& {Set-ExecutionPolicy bypass}"
powershell.exe ./solutionsetup.ps1

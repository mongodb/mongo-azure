#!/bin/bash

# Powershell wants to run from SETUPDIR
SETUPDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

cd $SETUPDIR

powershell.exe -Command "& {Set-ExecutionPolicy bypass}"

if [[ -z $1 ]]
then
    powershell.exe ./solutionsetup.ps1
else
    powershell.exe ./solutionsetup.ps1 $1
fi



<#
 # Copyright 2010-2012 10gen Inc.
 # file : CleanupRepository.ps1
 # Licensed under the Apache License, Version 2.0 (the "License");
 # you may not use this file except in compliance with the License.
 # You may obtain a copy of the License at
 # 
 # 
 # http://www.apache.org/licenses/LICENSE-2.0
 # 
 # 
 # Unless required by applicable law or agreed to in writing, software
 # distributed under the License is distributed on an "AS IS" BASIS,
 # WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 # See the License for the specific language governing permissions and
 # limitations under the License.
 #>
 
# Script needs to be run as administrator

$checkFile = Join-Path $pwd ".\.git\HEAD";
$baseSrcDir = Join-Path $pwd "src"
$baseToolsDir = Join-Path $pwd "Tools"
$directoriesToClean = @((Join-Path $baseToolsDir "CheckinVerifier\MongoDB.WindowsAzure.Tools.CheckinVerifier"), 
    (Join-Path $baseToolsDir "BlobBackup"), 
    (Join-Path $baseSrcDir "MongoDB.WindowsAzure.Backup"),
    (Join-Path $baseSrcDir "MongoDB.WindowsAzure.Common"), 
    (Join-Path $baseSrcDir "MongoDB.WindowsAzure.Deploy"),
    (Join-Path $baseSrcDir "MongoDB.WindowsAzure.InstanceMaintainer"), 
    (Join-Path $baseSrcDir "MongoDB.WindowsAzure.Manager"), 
    (Join-Path $baseSrcDir "MongoDB.WindowsAzure.MongoDBRole"), 
    (Join-Path $baseSrcDir "SampleApplications\MvcMovieSample\MongoDB.WindowsAzure.Sample.MvcMovie"))
$directoriesToCleanCsx = @((Join-Path $baseSrcDir "MongoDB.WindowsAzure.Deploy"), 
(Join-Path $baseSrcDir "SampleApplications\MvcMovieSample\MongoDB.WindowsAzure.Sample.Deploy"))

function CleanupDirectory {
    Param($basePath, $additionalPath)
    $dirToClean = Join-Path $basePath $additionalPath
    if (Test-Path -path $dirToClean) {
        Remove-Item -Force -Recurse $dirToClean
    }
}

function CleanBuildDirectories {
    foreach ($subdir in @("obj","bin")) {
        foreach ($directory in $directoriesToClean) {
            CleanupDirectory $directory $subdir
        }
    }

    foreach ($subdir in @("obj","bin", "csx")) {
        foreach ($directory in $directoriesToCleanCsx) {
            CleanupDirectory $directory $subdir
        }
    }
}

function CleanWaddStore {
    $dirToClean = Join-Path $env:LOCALAPPDATA 'dftmp\wadd\devstoreaccount1'
    CleanupDirectory $dirToClean
}

function Test-Administrator  
{  
    $user = [Security.Principal.WindowsIdentity]::GetCurrent();
    (New-Object Security.Principal.WindowsPrincipal $user).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator) 
}

if (!(Test-Administrator)) {
    Write-Host -BackgroundColor White -ForegroundColor Red "Script needs to be run as administrator"
    exit -1  
}

if (!(Test-Path -path $checkFile -PathType Leaf)) {
    Write-Host -BackgroundColor White -ForegroundColor Red "Script needs to be run from project git root"
    exit -1
}

CleanBuildDirectories
CleanWaddStore

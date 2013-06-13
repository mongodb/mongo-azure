<#
 # Copyright 2010-2011 10gen Inc.
 # file : solutionsetup.ps1
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
 
[CmdletBinding()]
Param(
   [ValidatePattern("^[2]\.[2-5]\.[0-9]$|^latest$|^v[2]\.[2|2|4|]\-latest$")]
   [alias("v")]
   [string]$Version = "2.4.4",

   [alias("f")]
   [switch]$OverwriteBinaries
)

$coreCloudConfigTemplateFile = Join-Path $pwd ".\ServiceConfiguration.Cloud.cscfg.core"
$coreCloudConfigFile = Join-Path $pwd "..\src\MongoDB.WindowsAzure.Deploy\ServiceConfiguration.Cloud.cscfg"
$sampleCloudConfigTemplateFile = Join-Path $pwd ".\ServiceConfiguration.Cloud.cscfg.sample"
$sampleCloudConfigFile = Join-Path $pwd "..\src\SampleApplications\MvcMovieSample\MongoDB.WindowsAzure.Sample.Deploy\ServiceConfiguration.Cloud.cscfg"

$mongodbDownloadUrl = "http://dl.mongodb.org/special/azure-paas-64.zip"
$mongodbBinaryTarget = Join-Path $pwd "..\lib\MongoDBBinaries"
$mongodExe = Join-Path (Join-Path $mongodbBinaryTarget "bin") "mongod.exe"
$mongodbDownloadUrlString = "http://downloads.mongodb.org/win32/mongodb-win32-x86_64-2008plus-{0}.zip"

function Setup-CoreCloudConfig {
    Write-Verbose "Creating Cloud config file for core project"
    if (!(Test-Path -LiteralPath $coreCloudConfigFile -PathType Leaf)) {
        cp $coreCloudConfigTemplateFile $coreCloudConfigFile
        Write-Host "Cloud config file created for core project"
    }
    else {
        Write-Warning "Cloud config for core already exists. Not overwriting"
    }
}

function Setup-SampleCloudConfig {
    Write-Verbose "Creating Cloud config file for sample project"
    if (!(Test-Path -LiteralPath $sampleCloudConfigFile -PathType Leaf)) {
        cp $sampleCloudConfigTemplateFile $sampleCloudConfigFile
        Write-Host "Cloud config file created for sample project"
    }
    else {
        Write-Warning "Cloud config for sample already exists. Not overwriting"
    }
}

function Download-Binaries {
    Param([string]$downloadUrl, [bool]$overwrite)

    if ((Test-Path -LiteralPath $mongodExe -PathType Leaf) -and
        ($overwrite -eq $false)) {
        Write-Warning  $mongodExe" already exists. Not overwriting"
        return
    }
    
    $storageDir = Join-Path $pwd "downloadtemp"
    $webclient = New-Object System.Net.WebClient
    $split = $downloadUrl.split("/")
    $fileName = $split[$split.Length-1]
    $filePath = Join-Path $storageDir $fileName
    
    if (!(Test-Path -LiteralPath $storageDir -PathType Container)) {
        New-Item -type directory -path $storageDir | Out-Null
    }
    else {
        Write-Verbose "Cleaning out temporary download directory"
        Remove-Item (Join-Path $storageDir "*") -Recurse -Force
        Write-Verbose "Temporary download directory cleaned"
    }
    if (Test-Path -LiteralPath $mongodbBinaryTarget) {
        Remove-Item -Recurse $mongodbBinaryTarget
    }
    
    Write-Verbose "Downloading MongoDB binaries from $downloadUrl. This could take time..."
    $webclient.DownloadFile($downloadUrl, $filePath)
    Write-Verbose "MongoDB binaries downloaded. Unzipping..."
    
    $shell_app=new-object -com shell.application
    $zip_file = $shell_app.namespace($filePath)
    $destination = $shell_app.namespace($storageDir)
    
    $destination.Copyhere($zip_file.items())
    
    Write-Verbose "Binaries unzipped. Copying to $mongodbBinaryTarget"
    $unzipDir = GetUnzipPath $storageDir $fileName
    Copy-Item $unzipDir -destination $mongodbBinaryTarget -Recurse
    Write-Verbose "Done copying. Clearing temporary storage directory $storageDir"
    
    if (Test-Path -LiteralPath $storageDir -PathType Container) {
        Remove-Item -path $storageDir -force -Recurse
    }
    Write-Host "Done downloading MongoDB binaries"
}

function GetUnzipPath {
    Param([string]$downloadDir, [string]$downloadedFile)
    $dir = Get-Item (Join-Path $downloadDir "*") -Exclude $downloadedFile
    return $dir.FullName
}

$downloadUrl = $mongodbDownloadUrlString -f $Version

Write-Host "Downloading MongoDB $Version from $downloadUrl"

Write-Verbose "Start with setup.."
Setup-CoreCloudConfig
Setup-SampleCloudConfig
Download-Binaries $downloadUrl $OverwriteBinaries
Write-Debug "Done with setup"

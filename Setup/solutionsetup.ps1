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
 
$coreCloudConfigTemplateFile = Join-Path $pwd ".\ServiceConfiguration.Cloud.cscfg.core"
$coreCloudConfigFile = Join-Path $pwd "..\src\MongoDB.WindowsAzure.Deploy\ServiceConfiguration.Cloud.cscfg"
$sampleCloudConfigTemplateFile = Join-Path $pwd ".\ServiceConfiguration.Cloud.cscfg.sample"
$sampleCloudConfigFile = Join-Path $pwd "..\src\SampleApplications\MvcMovieSample\MongoDB.WindowsAzure.Sample.Deploy\ServiceConfiguration.Cloud.cscfg"

$mongodbDownloadUrl = "http://dl.mongodb.org/special/azure-paas-64.zip"
$mongodbBinaryTarget = Join-Path $pwd "..\lib\MongoDBBinaries"
$mongodExe = Join-Path (Join-Path $mongodbBinaryTarget "bin") "mongod.exe"
$mongodbDownloadUrlString = "http://downloads.mongodb.org/win32/mongodb-win32-x86_64-2008plus-{0}.zip"
$mongodbCurrentStableVersion = "2.4.4"

function Setup-CoreCloudConfig {
    Write-Host "Creating Cloud config file for core project"
    if (!(Test-Path -LiteralPath $coreCloudConfigFile -PathType Leaf)) {
        cp $coreCloudConfigTemplateFile $coreCloudConfigFile
    }
    Write-Host "Cloud config file created for core project"
}

function Setup-SampleCloudConfig {
    Write-Host "Creating Cloud config file for sample project"
    if (!(Test-Path -LiteralPath $sampleCloudConfigFile -PathType Leaf)) {
        cp $sampleCloudConfigTemplateFile $sampleCloudConfigFile
    }
    Write-Host "Cloud config file created for sample project"
}

function Download-Binaries {
    Param($downloadUrl)

    if (Test-Path -LiteralPath $mongodExe -PathType Leaf) {
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
        Write-Host "Cleaning out temporary download directory"
        Remove-Item (Join-Path $storageDir "*") -Recurse -Force
        Write-Host "Temporary download directory cleaned"
    }
    if (Test-Path -LiteralPath $mongodbBinaryTarget) {
        Remove-Item -Recurse $mongodbBinaryTarget
    }
    
    Write-Host "Downloading MongoDB binaries from" $downloadUrl". This could take time..."
    $webclient.DownloadFile($downloadUrl, $filePath)
    Write-Host "MongoDB binaries downloaded. Unzipping..."
    
    $shell_app=new-object -com shell.application
    $zip_file = $shell_app.namespace($filePath)
    $destination = $shell_app.namespace($storageDir)
    
    $destination.Copyhere($zip_file.items())
    
    Write-Host "Binaries unzipped. Copying to" $mongodbBinaryTarget
    $unzipDir = GetUnzipPath $storageDir $fileName
    Copy-Item $unzipDir -destination $mongodbBinaryTarget -Recurse
    Write-Host "Done copying. Clearing temporary storage directory" $storageDir
    
    if (Test-Path -LiteralPath $storageDir -PathType Container) {
        Remove-Item -path $storageDir -force -Recurse
    }
    
}

function GetUnzipPath {
    Param([string]$downloadDir, [string]$downloadedFile)
    $dir = Get-Item (Join-Path $downloadDir "*") -Exclude $downloadedFile
    return $dir.FullName
}

$downloadUrl = $mongodbDownloadUrlString -f $mongodbCurrentStableVersion
if ($args.Length -gt 0) {
    $versionSuffix = $args[0]
    if ($versionSuffix.Contains("latest") -and
        ($versionSuffix -ne"latest")) {
        $downloadUrl = $mongodbDownloadUrlString -f ("v"+$versionSuffix)
    }
    else {
        $downloadUrl = $mongodbDownloadUrlString -f $versionSuffix
    }
}

Write-Host "Start with setup.."
Setup-CoreCloudConfig
Setup-SampleCloudConfig
Download-Binaries $downloadUrl
Write-Host "Done with setup"
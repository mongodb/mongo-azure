﻿/*
 * Copyright 2010-2012 10gen Inc.
 * file : Program.cs
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace MongoDB.WindowsAzure.Tools.BlobBackup
{
    using System;
    using Microsoft.WindowsAzure;
    using MongoDB.WindowsAzure.Tools.BlobBackup.Properties;

    class Program
    {
        static void Main( string[] args )
        {
            Console.WriteLine( "BlobBackup v1.04 starting..." );

            if ( string.IsNullOrEmpty( Settings.Default.StorageName ) || string.IsNullOrEmpty( Settings.Default.StorageKey ) )
            {
                Console.WriteLine( "Error: specify your Azure Storage Credentials in this program's configuration file." );
                return;
            }

            var credentials = new StorageCredentialsAccountAndKey( Settings.Default.StorageName, Settings.Default.StorageKey );
            BackupEngine.Backup( credentials );
        }
    }
}
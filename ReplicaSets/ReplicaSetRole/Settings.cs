/*
 * Copyright 2010-2011 10gen Inc.
 * file : Constants.cs
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

namespace MongoDB.Azure.ReplicaSets.ReplicaSetRole
{

    using System;
    using System.Text.RegularExpressions;

    using Microsoft.WindowsAzure.ServiceRuntime;

    internal static class Settings
    {
        #region DO NOT MODIFY
        internal const string MongodDataBlobContainerName = "mongoddatadrive{0}";
        internal const string MongodDataBlobName = "mongoddblob{0}.vhd";

        internal const string MongodCloudDataDir = "MongoDBDataDir";
        internal const string MongodDataDirSize = "MongoDataDirSize";
        internal const string MongodLocalDataDir = "MongoDBLocalDataDir";
        internal const string MongodLogDir = "MongodLogDir";
        internal const string DiagnosticsConnectionString = "DiagnosticsConnectionString";
        internal const string MongoDBLogVerbosity = "MongoDBLogVerbosity";

        internal const string MongoDBBinaryFolder = @"approot\MongoDBBinaries\bin";
        internal const string MongodLogFileName = "mongod.log";
        internal const string MongodCommandLineCloud = "--port {0} --dbpath {1} --logpath {2} --nohttpinterface --logappend --replSet {3} {4}";
        internal const string MongodCommandLineEmulated = "--port {0} --dbpath {1} --logpath {2} --replSet {3} {4}";

        internal const string MongodDataBlobCacheDir = "MongodDataBlobCacheDir";

        // Default values for configurable settings
        private const int DefaultEmulatedDBDriveSize = 1024; // in MB
        private const int DefaultDeployedDBDriveSize = 100 * 1024; // in MB

        private static readonly Regex logLevelRegex = new Regex("^(-?)([v]*)$");

        #endregion DO NOT MODIFY

        #region Configurable Section
        internal static readonly TimeSpan DiagnosticTransferInterval = TimeSpan.FromMinutes(1);
        internal static readonly TimeSpan PerfCounterTransferInterval = TimeSpan.FromMinutes(1);
        #endregion Configurable Section

        internal static readonly int MaxDBDriveSize; // in MB
        internal static string MongodLogLevel = "-v";

        static Settings()
        {
            int dbDriveSize;
            if (RoleEnvironment.IsEmulated)
            {
                dbDriveSize = DefaultEmulatedDBDriveSize;
            }
            else
            {
                dbDriveSize = DefaultDeployedDBDriveSize;
            }

            string mongoDataDirSize = null; 
            try 
            {
                mongoDataDirSize = RoleEnvironment.GetConfigurationSettingValue(MongodDataDirSize);
            }
            catch (RoleEnvironmentException)
            {
                // setting does not exist use default
            }
            catch (Exception)
            {
                // setting does not exist?
            }

            if (!string.IsNullOrEmpty(mongoDataDirSize))
            {
                int parsedDBDriveSize = 0;
                if (int.TryParse(mongoDataDirSize, out parsedDBDriveSize))
                {
                    MaxDBDriveSize = parsedDBDriveSize;
                }
                else
                {
                    MaxDBDriveSize = dbDriveSize;
                }
            }
            else
            {
                MaxDBDriveSize = dbDriveSize;
            }

            string configuredLogLevel = null;
            try
            {
                configuredLogLevel = RoleEnvironment.GetConfigurationSettingValue(Settings.MongoDBLogVerbosity);
            }
            catch (RoleEnvironmentException)
            {
                // setting does not exist use default
            }
            catch (Exception)
            {
                // setting does not exist?
            }

            if (!string.IsNullOrEmpty(configuredLogLevel))
            {
                Match m = logLevelRegex.Match(configuredLogLevel);
                if (m.Success)
                {
                    MongodLogLevel = string.IsNullOrEmpty(m.Groups[1].ToString()) ?
                        "-" + m.Groups[0].ToString() :
                        m.Groups[0].ToString();
                }

            }
        }

    }
}

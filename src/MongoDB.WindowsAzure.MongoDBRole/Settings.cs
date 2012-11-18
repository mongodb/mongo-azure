/*
 * Copyright 2010-2012 10gen Inc.
 * file : Settings.cs
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

namespace MongoDB.WindowsAzure.MongoDBRole
{
    using Microsoft.WindowsAzure.ServiceRuntime;

    using MongoDB.WindowsAzure.Common;

    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal static class Settings
    {
        #region DO NOT MODIFY

        // configuration setting names
        // TODO move any shared ones to CommonSettings
        internal const string LocalDataDirSetting = "MongoDBLocalDataDir";
        internal const string DataDirSizeMBSetting = "MongoDBDataDirSizeMB";
        internal const string LogDirSetting = "MongodLogDir";
        internal const string LogVerbositySetting = "MongoDBLogVerbosity";
        internal const string RecycleOnExitSetting = "RecycleOnExit";

        internal const string MongoDBBinaryFolder = @"approot\MongoDBBinaries\bin";
        internal const string MongodLogFileName = "mongod.log";
        internal const string MongodCommandLineCloud = "--port {0} --dbpath {1} --logpath {2} --nohttpinterface --logappend --replSet {3} {4}";
        internal const string MongodCommandLineEmulated = "--port {0} --dbpath {1} --logpath {2} --replSet {3} {4} --oplogSize 100 --smallfiles --noprealloc";

        internal const string LocalHostString = "localhost:{0}";

        internal static readonly string[] ExemptConfigurationItems =
            new[] { LogVerbositySetting, RecycleOnExitSetting };


        // Default values for configurable settings
        private const int DefaultEmulatedDBDriveSize = 1024; // in MB
        private const int DefaultDeployedDBDriveSize = 100 * 1024; // in MB

        #endregion DO NOT MODIFY

        internal static readonly int DataDirSizeMB = GetDataDirSizeMB(); // in MB
        internal static string MongodLogLevel = GetLogVerbosity();
        internal static bool RecycleOnExit = GetRecycleOnExit();

        internal static string GetLogVerbosity()
        {
            string value;

            if (!TryGetRoleConfigurationSettingValue(
                Settings.LogVerbositySetting,
                out value) ||
                value.Length == 0)
            {
                return null;
            }

            var m = Regex.Match(value, "^-?(v+)$");
            if (!m.Success)
            {
                ThrowInvalidConfigurationSetting(
                    Settings.LogVerbositySetting,
                    value);
            }

            return "-" + m.Groups[1].Value;
        }

        internal static bool GetRecycleOnExit()
        {
            string value;

            if (!TryGetRoleConfigurationSettingValue(
                Settings.RecycleOnExitSetting,
                out value) ||
                value.Length == 0)
            {
                return true;
            }

            bool recycleOnExit;

            if (!bool.TryParse(value, out recycleOnExit))
            {
                ThrowInvalidConfigurationSetting(
                    Settings.RecycleOnExitSetting,
                    value);
            }

            return recycleOnExit;
        }

        private static int GetDataDirSizeMB()
        {
            string value;

            if (!TryGetRoleConfigurationSettingValue(
                    DataDirSizeMBSetting,
                    out value) ||
                value.Length == 0)
            {
                return RoleEnvironment.IsEmulated ?
                    DefaultEmulatedDBDriveSize : DefaultDeployedDBDriveSize;
            }

            int dataDirSizeMB;

            if (!int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out dataDirSizeMB))
            {
                ThrowInvalidConfigurationSetting(
                    DataDirSizeMBSetting,
                    value);
            }

            return dataDirSizeMB;
        }

        private static bool TryGetRoleConfigurationSettingValue(
            string configurationSettingName,
            out string value)
        {
            try
            {
                value = RoleEnvironment.GetConfigurationSettingValue(configurationSettingName);
            }
            catch (RoleEnvironmentException)
            {
                value = null;

                return false;
            }

            return true;
        }

        private static void ThrowInvalidConfigurationSetting(
            string name,
            string value)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                "Configuration setting name '{0}' has invalid value \"{1}\"",
                name,
                value);

            throw new ConfigurationErrorsException(message);
        }
    }
}

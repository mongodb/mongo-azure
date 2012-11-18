/*
 * Copyright 2010-2012 10gen Inc.
 * file : MongoDBRole.cs
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

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    using MongoDB.WindowsAzure.Common;
    using MongoDB.Driver;

    public class MongoDBRole : RoleEntryPoint
    {
        private Process mongodProcess;
        private CloudDrive mongoDataDrive;
        private string mongodHost;
        private int mongodPort;
        private string mongodDataDriveLetter;
        private string replicaSetName;
        private ManualResetEvent stopEvent;
        private int instanceId;

        public override void Run()
        {
            using (DiagnosticsHelper.TraceMethod())
            {
                this.mongodProcess.WaitForExit();

                if (!Settings.RecycleOnExit)
                {
                    this.stopEvent.WaitOne();
                }
            }
        }

        public override bool OnStart()
        {
            using (DiagnosticsHelper.TraceMethod())
            {
                // For information on handling configuration changes
                // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

                // Set the maximum number of concurrent connections
                ServicePointManager.DefaultConnectionLimit = 12;

                CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
                {
                    configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
                });

                RoleEnvironment.Changing += RoleEnvironmentChanging;
                RoleEnvironment.Changed += RoleEnvironmentChanged;

                replicaSetName = RoleEnvironment.GetConfigurationSettingValue(Constants.ReplicaSetNameSetting);
                instanceId = ConnectionUtilities.ParseNodeInstanceId(RoleEnvironment.CurrentRoleInstance.Id);

                DiagnosticsHelper.TraceInformation("ReplicaSetName={0}", replicaSetName);

                SetHostAndPort();
                DiagnosticsHelper.TraceInformation("Obtained host={0}, port={1}", mongodHost, mongodPort);

                StartMongoD();
                DiagnosticsHelper.TraceInformation("Mongod process started");

                // Need to ensure MongoD is up here
                DatabaseHelper.EnsureMongodIsListening(replicaSetName, instanceId, mongodPort);

                if ((instanceId == 0) && !DatabaseHelper.IsReplicaSetInitialized(mongodPort))
                {
                    try
                    {
                        DatabaseHelper.RunInitializeCommandLocally(replicaSetName, mongodPort);
                        DiagnosticsHelper.TraceInformation("RSInit issued successfully");
                    }
                    catch (MongoCommandException e)
                    {
                        //Ignore exceptions caught on rs init for now
                        DiagnosticsHelper.TraceWarningException("Exception on RSInit", e);
                    }
                }

                this.stopEvent = new ManualResetEvent(false);
            }

            return true;
        }

        public override void OnStop()
        {
            using (DiagnosticsHelper.TraceMethod())
            {
                try
                {
                    if ((mongodProcess != null) &&
                        !(mongodProcess.HasExited))
                    {
                        DatabaseHelper.StepdownIfNeeded(mongodPort);
                    }
                }
                catch (MongoCommandException e)
                {
                    //Ignore exceptions caught on unmount
                    DiagnosticsHelper.TraceWarningException("Exception in onstop - stepdown failed", e);
                }

                try
                {
                    if ((mongodProcess != null) &&
                        !(mongodProcess.HasExited))
                    {
                        this.ShutdownMongoD();
                    }
                }
                catch (MongoCommandException e)
                {
                    //Ignore exceptions caught on unmount
                    DiagnosticsHelper.TraceWarningException("Exception in onstop - mongo shutdown", e);
                }

                try
                {
                    DiagnosticsHelper.TraceInformation("Unmount called on data drive");
                    if (mongoDataDrive != null)
                    {
                        mongoDataDrive.Unmount();
                    }
                    DiagnosticsHelper.TraceInformation("Unmount completed on data drive");
                }
                catch (CloudDriveException e)
                {
                    //Ignore exceptions caught on unmount
                    DiagnosticsHelper.TraceWarningException("Exception in onstop - unmount of data drive", e);
                }

                if (this.stopEvent != null)
                {
                    this.stopEvent.Set();
                }
            }
        }

        private void SetHostAndPort()
        {
            var endPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[Constants.MongodPortSetting].IPEndpoint;
            mongodHost = endPoint.Address.ToString();
            mongodPort = endPoint.Port;
            if (RoleEnvironment.IsEmulated)
            {
                mongodPort += instanceId;
            }
        }

        private void ShutdownMongoD()
        {
            using (DiagnosticsHelper.TraceMethod())
            {
                var server = DatabaseHelper.GetLocalSlaveOkConnection(mongodPort);
                server.Shutdown();
            }
        }

        private void StartMongoD()
        {
            using (DiagnosticsHelper.TraceMethod())
            {
                var mongoAppRoot = Path.Combine(
                    Environment.GetEnvironmentVariable("RoleRoot") + @"\",
                    Settings.MongoDBBinaryFolder);
                var mongodPath = Path.Combine(mongoAppRoot, @"mongod.exe");

                var blobPath = GetMongoDataDirectory();

                var logFile = GetLogFile();

                var logLevel = Settings.MongodLogLevel;

                string cmdline;
                if (RoleEnvironment.IsEmulated)
                {
                    cmdline = String.Format(Settings.MongodCommandLineEmulated,
                        mongodPort,
                        blobPath,
                        logFile,
                        replicaSetName,
                        logLevel);
                }
                else
                {
                    cmdline = String.Format(Settings.MongodCommandLineCloud,
                        mongodPort,
                        blobPath,
                        logFile,
                        replicaSetName,
                        logLevel);
                }

                DiagnosticsHelper.TraceInformation("Launching mongod as \"{0}\" {1}", mongodPath, cmdline);

                // launch mongo
                try
                {
                    mongodProcess = new Process()
                    {
                        StartInfo = new ProcessStartInfo(mongodPath, cmdline)
                        {
                            UseShellExecute = false,
                            WorkingDirectory = mongoAppRoot,
                            CreateNoWindow = false
                        }
                    };
                    mongodProcess.Start();
                }
                catch (Exception e)
                {
                    DiagnosticsHelper.TraceErrorException("Can't start Mongo", e);
                    throw; // throwing an exception here causes the VM to recycle
                }
            }
        }

        private string GetMongoDataDirectory()
        {
            using (DiagnosticsHelper.TraceMethod())
            {
                var dataBlobName = string.Format(Constants.MongoDataBlobName, instanceId);
                var containerName = ConnectionUtilities.GetDataContainerName(replicaSetName);
                mongodDataDriveLetter = Utilities.GetMountedPathFromBlob(
                    Settings.LocalDataDirSetting,
                    Constants.MongoDataCredentialSetting,
                    containerName,
                    dataBlobName,
                    Settings.DataDirSizeMB,
                    out mongoDataDrive);
                DiagnosticsHelper.TraceInformation("Obtained azure drive, mounted as \"{0}\"", mongodDataDriveLetter);
                var dir = Directory.CreateDirectory(Path.Combine(mongodDataDriveLetter, @"data"));
                DiagnosticsHelper.TraceInformation("Data directory is \"{0}\"", dir.FullName);
                return dir.FullName;
            }
        }

        private string GetLogFile()
        {
            using (DiagnosticsHelper.TraceMethod())
            {
                var localStorage = RoleEnvironment.GetLocalResource(Settings.LogDirSetting);
                var logfile = Path.Combine(localStorage.RootPath + @"\", Settings.MongodLogFileName);
                return ("\"" + logfile + "\"");
            }
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            Func<RoleEnvironmentConfigurationSettingChange, bool> changeIsExempt =
                x => !Settings.ExemptConfigurationItems.Contains(x.ConfigurationSettingName);
            var environmentChanges = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>();
            e.Cancel = environmentChanges.Any(changeIsExempt);
            DiagnosticsHelper.TraceInformation("Role config changing. Cancel set to {0}",
                e.Cancel);
        }

        private void RoleEnvironmentChanged(object sender, RoleEnvironmentChangedEventArgs e)
        {
            // Get the list of configuration setting changes
            var settingChanges = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>();

            foreach (var settingChange in settingChanges)
            {
                var settingName = settingChange.ConfigurationSettingName;

                DiagnosticsHelper.TraceInformation(
                    "Setting '{0}' now has value \"{1}\"",
                    settingName,
                    RoleEnvironment.GetConfigurationSettingValue(settingName));

                switch (settingName)
                {
                    case Settings.LogVerbositySetting:
                        var logLevel = Settings.GetLogVerbosity();
                        if (logLevel != null)
                        {
                            if (logLevel != Settings.MongodLogLevel)
                            {
                                Settings.MongodLogLevel = logLevel;
                                DatabaseHelper.SetLogLevel(mongodPort, logLevel);
                            }
                        }
                        break;

                    case Settings.RecycleOnExitSetting:
                        Settings.RecycleOnExit = Settings.GetRecycleOnExit();
                        break;
                }
            }

            // Get the list of topology changes
            var topologyChanges = e.Changes.OfType<RoleEnvironmentTopologyChange>();

            foreach (var topologyChange in topologyChanges)
            {
                var roleName = topologyChange.RoleName;
                DiagnosticsHelper.TraceInformation(
                    "Role {0} now has {1} instance(s)",
                    roleName,
                    RoleEnvironment.Roles[roleName].Instances.Count);
            }
        }
    }
}

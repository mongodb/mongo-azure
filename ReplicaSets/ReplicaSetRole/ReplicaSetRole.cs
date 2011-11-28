/*
 * Copyright 2010-2011 10gen Inc.
 * file : ReplicaSetRole.cs
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
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    using MongoDB.Azure.ReplicaSets.MongoDBHelper;
    using MongoDB.Driver;

    public class ReplicaSetRole : RoleEntryPoint
    {

        private Process mongodProcess = null;
        private CloudDrive mongoDataDrive = null;
        private CloudDrive mongoLogDrive = null;
        private string mongodHost;
        private int mongodPort;
        private string mongodDataDriveLetter = null;
        private string replicaSetName = null;
        private int instanceId;

        public override void Run()
        {
            DiagnosticsHelper.TraceInformation("MongoWorkerRole run method called");
            var mongodRunning = CheckIfMongodRunning();

            while (mongodRunning)
            {
                ReplicaSetHelper.RunCloudCommandLocally(instanceId, mongodPort);
                Thread.Sleep(15000);
                mongodRunning = CheckIfMongodRunning();
            }

            Thread.Sleep(60000);

            DiagnosticsHelper.TraceWarning("MongoWorkerRole run method exiting");
        }

        public override bool OnStart()
        {
            DiagnosticsHelper.TraceInformation("MongoWorkerRole onstart called");

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });

            replicaSetName = RoleEnvironment.GetConfigurationSettingValue(MongoDBHelper.ReplicaSetNameSetting);
            instanceId = MongoDBHelper.ParseNodeInstanceId(RoleEnvironment.CurrentRoleInstance.Id);

            DiagnosticsHelper.TraceInformation(string.Format("ReplicaSetName={0}, InstanceId={1}",
                replicaSetName, instanceId));

            SetHostAndPort();
            DiagnosticsHelper.TraceInformation(string.Format("Obtained host={0}, port={1}", mongodHost, mongodPort));

            StartMongoD();

            DiagnosticsHelper.TraceInformation("Mongod process started");

            var commandSucceeded = false;
            while (!commandSucceeded)
            {
                try
                {
                    ReplicaSetHelper.RunCloudCommandLocally(instanceId, mongodPort);
                    commandSucceeded = true;
                }
                catch (Exception e)
                {
                    DiagnosticsHelper.TraceInformation(e.Message);
                    commandSucceeded = false;
                    Thread.Sleep(1000);
                }
            }

            DiagnosticsHelper.TraceInformation("Cloud command done on OnStart");

            if (!ReplicaSetHelper.IsReplicaSetInitialized(mongodPort))
            {
                ReplicaSetHelper.RunInitializeCommandLocally(replicaSetName, mongodPort);
                DiagnosticsHelper.TraceInformation("RSInit issued");
            }

            DiagnosticsHelper.TraceInformation("Done with OnStart");
            return base.OnStart();
        }

        public override void OnStop()
        {
            DiagnosticsHelper.TraceInformation("MongoWorkerRole onstop called");
            try
            {
                // should we instead call Process.stop?
                DiagnosticsHelper.TraceInformation("Shutdown called on mongod");
                if ((mongodProcess != null) &&
                    !(mongodProcess.HasExited))
                {
                    ReplicaSetHelper.StepdownIfNeeded(mongodPort);
                    ShutdownMongo();
                }
                DiagnosticsHelper.TraceInformation("Shutdown completed on mongod");
            }
            catch (Exception e)
            {
                //Ignore exceptions caught on unmount
                DiagnosticsHelper.TraceWarning("Exception in onstop - mongo shutdown");
                DiagnosticsHelper.TraceWarning(e.Message);
                DiagnosticsHelper.TraceWarning(e.StackTrace);
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
            catch (Exception e)
            {
                //Ignore exceptions caught on unmount
                DiagnosticsHelper.TraceWarning("Exception in onstop - unmount of data drive");
                DiagnosticsHelper.TraceWarning(e.Message);
                DiagnosticsHelper.TraceWarning(e.StackTrace);
            }

            try
            {
                DiagnosticsHelper.TraceInformation("Unmount called on log drive");
                if (mongoLogDrive != null)
                {
                    mongoLogDrive.Unmount();
                }
                DiagnosticsHelper.TraceInformation("Unmount completed on log drive");
            }
            catch (Exception e)
            {
                //Ignore exceptions caught on unmount
                DiagnosticsHelper.TraceWarning("Exception in onstop - unmount of log drive");
                DiagnosticsHelper.TraceWarning(e.Message);
                DiagnosticsHelper.TraceWarning(e.StackTrace);
            }

            DiagnosticsHelper.TraceInformation("Calling diagnostics shutdown");
            // DiagnosticsHelper.ShutdownDiagnostics();
            base.OnStop();
        }

        private void SetHostAndPort()
        {
            var endPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[MongoDBHelper.MongodPortKey].IPEndpoint;
            mongodHost = endPoint.Address.ToString();
            mongodPort = endPoint.Port;
            if (RoleEnvironment.IsEmulated)
            {
                mongodPort += instanceId;
            }
        }

        private void ShutdownMongo()
        {
            var server = MongoDBHelper.GetLocalConnection(mongodPort);
            server.Shutdown();
        }

        private void StartMongoD()
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

            DiagnosticsHelper.TraceInformation(string.Format("Launching mongod as {0} {1}", mongodPath, cmdline));

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
                DiagnosticsHelper.TraceError("Can't start Mongo: " + e.Message);
                throw new ApplicationException("Can't start mongo: " + e.Message); // throwing an exception here causes the VM to recycle
            }
        }

        private string GetMongoDataDirectory()
        {
            DiagnosticsHelper.TraceInformation("Getting db path");
            var dataBlobName = string.Format(Settings.MongodDataBlobName, instanceId);
            var containerName = string.Format(Settings.MongodDataBlobContainerName, replicaSetName);
            mongodDataDriveLetter = Utilities.GetMountedPathFromBlob(
                Settings.MongodLocalDataDir,
                Settings.MongodCloudDataDir,
                containerName,
                dataBlobName,
                Settings.MaxDBDriveSize,
                out mongoDataDrive);
            DiagnosticsHelper.TraceInformation(string.Format("Obtained data drive as {0}", mongodDataDriveLetter));
            var dir = Directory.CreateDirectory(Path.Combine(mongodDataDriveLetter, @"data"));
            DiagnosticsHelper.TraceInformation(string.Format("Data directory is {0}", dir.FullName));
            return dir.FullName;
        }

        private string GetLogFile()
        {
            DiagnosticsHelper.TraceInformation("Getting log file base path");
            var localStorage = RoleEnvironment.GetLocalResource(Settings.MongodLogDir);
            var logfile = Path.Combine(localStorage.RootPath + @"\", Settings.MongodLogFileName);
            return logfile;
        }

        private bool CheckIfMongodRunning()
        {
            var processExited = mongodProcess.HasExited;
            return !processExited;
        }

    }
}

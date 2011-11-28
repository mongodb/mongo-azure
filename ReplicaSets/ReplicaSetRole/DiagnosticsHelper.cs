/*
 * Copyright 2010-2011 10gen Inc.
 * file : DiagnosticsHelper.cs
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

    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;

    using System;
    using System.Diagnostics;
    using System.IO;

    internal class DiagnosticsHelper
    {

        private static string messageFormat = "Instance-{0}:{1}";

        internal static void TraceInformation(string message)
        {
            try
            {
                Trace.TraceInformation(string.Format(
                    messageFormat,
                    RoleEnvironment.CurrentRoleInstance.Id,
                    message));
            }
            catch { }
        }

        internal static void TraceWarning(string message)
        {
            try
            {
                Trace.TraceWarning(string.Format(
                    messageFormat,
                    RoleEnvironment.CurrentRoleInstance.Id,
                    message));
            }
            catch { }
        }

        internal static void TraceError(string message)
        {
            try
            {
                Trace.TraceError(string.Format(
                    messageFormat,
                    RoleEnvironment.CurrentRoleInstance.Id,
                    message));
            }
            catch { }
        }

        static DiagnosticsHelper()
        {
            var diagObj = DiagnosticMonitor.GetDefaultInitialConfiguration();
            diagObj.Logs.ScheduledTransferPeriod = Settings.DiagnosticTransferInterval;
            AddPerfCounters(diagObj);
            diagObj.PerformanceCounters.ScheduledTransferPeriod = Settings.PerfCounterTransferInterval;
            diagObj.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            DiagnosticMonitor.Start(Settings.DiagnosticsConnectionString, diagObj);
        }

        private static void AddPerfCounters(DiagnosticMonitorConfiguration diagObj)
        {
            AddPerfCounter(diagObj, @"\LogicalDisk(*)\% Disk Read Time", 30);
            AddPerfCounter(diagObj, @"\LogicalDisk(*)\% Disk Write Time", 30);
            AddPerfCounter(diagObj, @"\LogicalDisk(*)\% Free Space", 30);
            AddPerfCounter(diagObj, @"\LogicalDisk(*)\Disk Read Bytes/sec", 30);
            AddPerfCounter(diagObj, @"\LogicalDisk(*)\Disk Write Bytes/sec", 30);
            AddPerfCounter(diagObj, @"\Memory\Available MBytes", 30);
            AddPerfCounter(diagObj, @"\Network Interface(*)\Bytes Received/sec", 30);
            AddPerfCounter(diagObj, @"\Network Interface(*)\Bytes Sent/sec", 30);
            AddPerfCounter(diagObj, @"\Processor(*)\% Processor Time", 30);
            AddPerfCounter(diagObj, @"\PhysicalDisk(*)\% Disk Read Time", 30);
            AddPerfCounter(diagObj, @"\PhysicalDisk(*)\% Disk Write Time", 30);
        }

        private static void AddPerfCounter(DiagnosticMonitorConfiguration config, string name, double seconds)
        {
            var perfmon = new PerformanceCounterConfiguration();
            perfmon.CounterSpecifier = name;
            perfmon.SampleRate = System.TimeSpan.FromSeconds(seconds);
            config.PerformanceCounters.DataSources.Add(perfmon);
        }
          
    }
}

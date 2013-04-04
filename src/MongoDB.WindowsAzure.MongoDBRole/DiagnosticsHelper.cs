/*
 * Copyright 2010-2013 10gen Inc.
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

namespace MongoDB.WindowsAzure.MongoDBRole
{

    using System;
    using System.Diagnostics;

    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.IO;
    using System.Security.Permissions;

    internal static class DiagnosticsHelper
    {

        private static TraceSource __traceSource = CreateTraceSource();

        internal static void TraceVerbose(string message)
        {
            __traceSource.TraceEvent(TraceEventType.Verbose, 0, message);
        }

        internal static void TraceVerbose(
            string message,
            params object[] args)
        {
            __traceSource.TraceEvent(
                TraceEventType.Verbose,
                0,
                message,
                args);
        }

        internal static void TraceInformation(string message)
        {
            __traceSource.TraceEvent(TraceEventType.Information, 0, message);
        }

        internal static void TraceInformation(
            string message, 
            params object[] args)
        {
            __traceSource.TraceEvent(
                TraceEventType.Information, 
                0, 
                message,
                args);
        }

        internal static void TraceWarning(string message)
        {
            __traceSource.TraceEvent(TraceEventType.Warning, 0, message);
        }

        internal static void TraceWarning(
            string message,
            params object[] args)
        {
            __traceSource.TraceEvent(
                TraceEventType.Warning,
                0,
                message,
                args);
        }

        internal static void TraceError(string message)
        {
            __traceSource.TraceEvent(TraceEventType.Error, 0, message);
        }

        internal static void TraceError(
            string message,
            params object[] args)
        {
            __traceSource.TraceEvent(
                TraceEventType.Error,
                0,
                message,
                args);
        }

        internal static void TraceCritical(string message)
        {
            __traceSource.TraceEvent(TraceEventType.Critical, 0, message);
        }

        internal static void TraceCritical(
            string message,
            params object[] args)
        {
            __traceSource.TraceEvent(
                TraceEventType.Critical,
                0,
                message,
                args);
        }

        private static TraceSource CreateTraceSource()
        {
            // Clone Trace but with an expanded name to include the current
            // role instance id.
            string traceSourceName;
            var roleInstanceId = RoleEnvironment.CurrentRoleInstance.Id;
            try
            {
                traceSourceName = Path.GetFileName(
                    Environment.GetCommandLineArgs()[0]) +
                    ": " +  roleInstanceId;
            }
            catch (NotSupportedException)
            {
                traceSourceName = "UNSUPPORTED: " + roleInstanceId;
            }

            var traceSource = new TraceSource(traceSourceName, SourceLevels.All);

            // Clear the default TraceSource listeners
            traceSource.Listeners.Clear();

            // Add the registered Trace listeners
            traceSource.Listeners.AddRange(Trace.Listeners);

            return traceSource;
        }

    }
}

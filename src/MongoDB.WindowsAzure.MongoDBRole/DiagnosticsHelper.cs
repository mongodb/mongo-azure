/*
 * Copyright 2010-2012 10gen Inc.
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

    using Microsoft.WindowsAzure.ServiceRuntime;

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    internal class DiagnosticsHelper
    {
        private static readonly string ExceptionMessageFormat =
            "{0}" + Environment.NewLine + "{1}";

        private static readonly Regex regexEscapeMessage =
            new Regex("[{}]", RegexOptions.Compiled);

        private static readonly TraceSource traceSource = CreateTraceSource();

        internal static IDisposable TraceMethod()
        {
            var stackFrame = new StackFrame(1);

            return TraceMethod(stackFrame.GetMethod().Name);
        }

        internal static IDisposable TraceMethod(string methodName)
        {
            return new TraceStartStop(methodName);
        }

        internal static void TraceStart(string message)
        {
            traceSource.TraceEvent(TraceEventType.Start, 0, message);
        }

        internal static void TraceStop(string message)
        {
            traceSource.TraceEvent(TraceEventType.Stop, 0, message);
        }

        internal static void TraceInformation(string message)
        {
            traceSource.TraceInformation(message);
        }

        internal static void TraceInformation(string format, params object[] args)
        {
            traceSource.TraceInformation(format, args);
        }

        internal static void TraceWarning(string message)
        {
            traceSource.TraceEvent(TraceEventType.Warning, 0, message);
        }

        internal static void TraceWarning(string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        internal static void TraceError(string message)
        {
            traceSource.TraceEvent(TraceEventType.Error, 0, message);
        }

        internal static void TraceError(string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Error, 0, format, args);
        }

        internal static void TraceInformationException(string message, Exception exception)
        {
            traceSource.TraceEvent(
                TraceEventType.Information,
                0,
                EscapeMessage(message) + "{0}",
                exception);
        }

        internal static void TraceWarningException(string message, Exception exception)
        {
            traceSource.TraceEvent(
                TraceEventType.Warning,
                0,
                EscapeMessage(message) + "{0}",
                exception);
        }

        internal static void TraceErrorException(string message, Exception exception)
        {
            traceSource.TraceEvent(
                TraceEventType.Error,
                0,
                EscapeMessage(message) + "{0}",
                exception);
        }

        private static TraceSource CreateTraceSource()
        {
            // Clone Trace but with an expanded name to include the current
            // role instance id.
            var name =
                Path.GetFileName(Environment.GetCommandLineArgs()[0]) +
                ": " +
                RoleEnvironment.CurrentRoleInstance.Id;

            var traceSource = new TraceSource(name, SourceLevels.All);

            // Clear the default TraceSource listeners
            traceSource.Listeners.Clear();

            // Add the registered Trace listeners
            traceSource.Listeners.AddRange(Trace.Listeners);

            return traceSource;
        }

        private static string EscapeMessage(string message)
        {
            // Replace all instances of { and } with {{ and }} respectively.
            return regexEscapeMessage.Replace(
                message,
                match => new string(match.Groups[0].Value[0], 2));
        }

        private static string GetAppName()
        {
            // Get the same value as
            // System.Diagnostics.TraceInternal.AppName.
            new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Assert();
            return Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        }

        private class TraceStartStop : IDisposable
        {
            private readonly string methodName;

            public TraceStartStop(string methodName)
            {
                DiagnosticsHelper.TraceStart(methodName);

                this.methodName = methodName;
            }

            #region IDisposable Members

            public void Dispose()
            {
                DiagnosticsHelper.TraceStop(this.methodName);

                GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}

/*
 * Copyright 2010-2011 10gen Inc.
 * file : ReplicaSetEnvironmentException.cs
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
 
namespace MongoDB.Azure.ReplicaSets
{
    using System;

    /// <summary>
    /// Exception indicating configuration issues such as the worker role name being changed.
    /// </summary>
    public class ReplicaSetEnvironmentException : Exception
    {
        /// <summary>
        /// Creates a new instance of ReplicaSetEnvironmentException.
        /// </summary>
        /// <param name="message">User visible error message.</param>
        /// <param name="innerException">Inner exception that caused this.</param>
        public ReplicaSetEnvironmentException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}

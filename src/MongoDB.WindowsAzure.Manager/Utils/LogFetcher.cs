/*
 * Copyright 2010-2013 10gen Inc.
 * file : LogFetcher.cs
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

namespace MongoDB.WindowsAzure.Manager.Src
{
    using System;
    using System.Web;

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    
    using MongoDB.WindowsAzure.Common;

    /// <summary>
    /// Writes the content of MongoDB log files to the HTTP response stream. 
    /// </summary>
    public class LogFetcher
    {
        /// <summary>
        /// Writes the log of the given instance number to the HTTP response.
        /// </summary>
        public static void WriteEntireLog(HttpResponseBase response, int instanceNum)
        {
            var blobName = String.Format(Constants.LogFileFormatString, 
                RoleEnvironment.DeploymentId, instanceNum);

            var storageAccount = CloudStorageAccount.Parse(
                RoleSettings.StorageCredentials);
            var client = storageAccount.CreateCloudBlobClient();
            var blob = client.GetContainerReference("wad-custom").
                GetBlobReference(blobName);

            CopyContents(blob, response);
        }

        /// <summary>
        /// Copies the full binary contents of the given blob to the given HTTP 
        /// response.
        /// </summary>
        private static void CopyContents(CloudBlob blob, 
            HttpResponseBase response, long offset = 0)
        {
            blob.FetchAttributes();

            response.BufferOutput = false;
            response.AddHeader("Content-Length", blob.Attributes.Properties.
                Length.ToString());
            response.Flush();

            using (var reader = blob.OpenRead())
            {                
                reader.Seek(offset, System.IO.SeekOrigin.Begin);

                byte[] buffer = new byte[1024 * 4]; // 4KB buffer
                while (reader.CanRead)
                {                    
                    int numBytes = reader.Read(buffer, 0, buffer.Length);

                    if (numBytes <= 0)
                        break;

                    response.BinaryWrite(buffer);
                    response.Flush();
                }
            }            
        }
    }
}
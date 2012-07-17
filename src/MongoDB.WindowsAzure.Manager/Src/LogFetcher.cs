using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using MongoDB.WindowsAzure.Common;
using System.IO;

namespace MongoDB.WindowsAzure.Manager.Src
{
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
            var credentials = RoleEnvironment.GetConfigurationSettingValue(Constants.MongoDataCredentialSetting);
            var blobName = String.Format("{0}/MongoDB.WindowsAzure.MongoDBRole/MongoDB.WindowsAzure.MongoDBRole_IN_{1}/mongod.log", RoleEnvironment.DeploymentId, instanceNum);

            var storageAccount = CloudStorageAccount.Parse(credentials);
            var client = storageAccount.CreateCloudBlobClient();
            var blob = client.GetContainerReference("wad-custom").GetBlobReference(blobName);

            CopyContents(blob, response);
        }

        /// <summary>
        /// Copies the full binary contents of the given blob to the given HTTP response.
        /// </summary>
        private static void CopyContents(CloudBlob blob, HttpResponseBase response, long offset = 0)
        {
            blob.FetchAttributes();

            response.BufferOutput = false;
            response.AddHeader("Content-Length", blob.Attributes.Properties.Length.ToString());
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
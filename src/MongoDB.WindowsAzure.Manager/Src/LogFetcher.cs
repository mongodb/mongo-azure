using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MongoDB.WindowsAzure.Manager.Src
{
    /// <summary>
    /// Fetches mongod logs from various instances...
    /// </summary>
    public class LogFetcher
    {
        public static string TailLog ( int instanceNum )
        {
            var credentials = "DefaultEndpointsProtocol=http;AccountName=managerstorage2;AccountKey=bHZHXu/6gNRq21YDjd3rOBqDw3sX7ldDl44rl8+x+Oz0KY4PYy1V5AD/dNQ7azS1i/NCQ+rwZAlNK65zDXAIyg==";
            var blobName = String.Format("{0}/MongoDB.WindowsAzure.MongoDBRole/MongoDB.WindowsAzure.MongoDBRole_IN_{1}/mongod.log", RoleEnvironment.DeploymentId, instanceNum);
         
            return GetContentsFromTail(GetBlob(credentials, blobName), 5000);
        }

        static CloudBlob GetBlob(string credentials, string blobName)
        {
            Console.WriteLine("Authing...");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(credentials);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            return client.GetContainerReference("wad-custom").GetBlobReference(blobName);
        }

        static string GetContents(CloudBlob blob, long offset, int length)
        {
            Console.WriteLine("Fetching contents...");
            byte[] buffer = new byte[length];
            using (var reader = blob.OpenRead())
            {
                reader.Seek(offset, System.IO.SeekOrigin.Begin);
                reader.Read(buffer, 0, length);
            }

            Console.WriteLine("Encoding...");
            return Encoding.UTF8.GetString(buffer);
        }

        static string GetContentsFromTail(CloudBlob blob, int length)
        {
            Console.WriteLine("Fetching attributes...");
            blob.FetchAttributes();
            long offset = Math.Max(0, blob.Attributes.Properties.Length - length);
            return GetContents(blob, offset, length);
        }
    }
}
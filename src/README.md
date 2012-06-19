## MongoDB Worker Role
Welcome to MongoDB on Azure Worker Roles    

## COMPONENTS
  * MongoDB.WindowsAzure.Common - Helper library (dll) that provides the necessary MongoDB driver wrapper functions. 
                Primarily used to obtain the connection string
  * MongoDB.WindowsAzure.MongoDBRole - The library that launches mongod as a member of a replica set. It is also responsible
                   for mounting the necessary blobs as cloud drives.
  * MongoDB.WindowsAzure.Deploy - The cloud project that provides the necessary configuration to deploy the above 
                      libraries to Azure.
  * MongoDB.WindowsAzure.InstanceMaintainer - Executable launched on role startup to maintain the instances
    * Currently updates the windows hosts file based on the instance IPs

## Maintainers
* Sridhar Nanjundeswaran    sridhar@10gen.com
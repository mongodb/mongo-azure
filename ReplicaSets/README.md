## MongoDB Replica Sets on Azure README
Welcome to MongoDB replica sets on Azure

## COMPONENTS
  * solutionsetup.cmd/ps1 - Scripts to be used in first time setup
  * MongoDBHelper - Helper library (dll) that provides the necessary MongoDB driver wrapper functions. 
                  Is used in the ReplicaSetRole but also by any .Net client applications to obtain the 
                  connection string. More details can be found in the API documentation.
  * ReplicaSetRole - The library that launches mongod as a member of a replica set. It is also responsible
                   for mounting the necessary blobs as cloud drives.
  * MongoDBReplicaSet - The cloud project that provides the necessary configuration to deploy the above 
                      libraries to Azure. It contains 2 configurations
    * ReplicaSetRole - Worker role config for the actual MongoDB replica set role
                     
## INITIAL SETUP
Run `solutionsetup.cmd` to setup up the solution for building. This script only needs to be run the first time.
This script does the following

  * Create ServiceConfiguration.Cloud.cscfg as a copy of configfiles/ServiceConfiguration.Cloud.cscfg.ref
  * Download mongodb binaries (currently 2.1.0-pre) to the appropriate solution location

### Prerequisites
  * Windows x64
  * .Net 4.0.
  * Visual Studio 2010 with SP1
    * Has been tested with Visual Web Developer Express and Visual Studio Ultimate
  * Windows Azure SDK 1.6 
  * Windows Azure Tools for Visual Studio 2010 1.6

### Build
  * Open MongoDBReplicaSet.sln from Visual Studio and build

## RUNNING

This section assumes you have already built a client application to access MongoDB running on Azure. See 
the sample application or refer to the wiki on how to build your own application against the solution.
More information can be found at the [configuration](http://www.mongodb.org/display/DOCS/Azure+Configuration)
and [deployment](http://www.mongodb.org/display/DOCS/Azure+Deployment) wikis

### Default configuration
  * 3 replica set members
  * replica set name is rs
  * Local cache for data drive is 1GB
  * 512MB log directory

### Deploying and running

The following steps describe how to configure, deploy and run MongoDB replica sets on Azure. More information can be found on the [wiki] 
(http://www.mongodb.org/display/DOCS/MongoDB+Replica+Sets+on+Azure)

### Running locally on compute/storage emulator
  * This should work out of the box and no special configuration is needed
  * The default configuration has
    * 3 replica set members running on ports 27017, 27018 and 27019
    * The default data directory size is 1GB (uses development storage)
    * Default log directory size is 512MB (local storage)
    * replica set name is rs

### Deploying to Azure
  * Create an Azure Storage account using the Azure Management Portal. You need the name and access
    keys to configure storage.
  * To deploy to the Azure cloud you need to edit the Cloud configuration and specify the cloud storage credentials
    from above
  * You can edit MongoDBReplicaSet\ServiceConfiguration.Cloud.cscfg or edit in Visual Studio
  * For ReplicaSetRole in the MongoDBReplicaSet cloud project, ensure the following are set
    * MongoDBDataDir - Set storage account credentials from above and use HTTP endpoints.
    * DiagnosticsConnectionString - Set storage account credentials from above and use HTTPS endpoints.

## Maintainers
* Sridhar Nanjundeswaran       sridhar@10gen.com

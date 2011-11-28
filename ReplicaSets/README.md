## MongoDB Replica Sets on Azure README
Welcome to MongoDB replica sets on Azure

## COMPONENTS
  * solutionsetup.ps1 - Powershell script to be used in first time setup
  * MongoDBHelper - Helper library (dll) that provides the necessary MongoDB driver wrapper functions. 
                  Is used in the ReplicaSetRole but also by any .Net client applications to obtain the 
                  connection string. More details can be found in the API document.
  * ReplicaSetRole - The library that launches mongod as a member of a replica set. It is also responsible
                   for mounting the necessary blobs as cloud drives.
  * MvcMovie - A sample MVC3 web app that demonstrates connecting and working with the mongodb replica set 
             deployed to Azure. In an actual user scenario this will be replaced with the user's actual app.
  * MongoDBReplicaSet - The cloud project that provides the necessary configuration to deploy the above 
                      libraries to Azure. It contains 2 configurations
    * MvcMovie - Web role config for the sample MvcMovie app
    * ReplicaSetRole - Worker role config for the actual MongoDB replica set role
                     
## INITIAL SETUP
Run `powershell .\solutionsetup.ps1` to setup up the solution for building. This script only needs to be run the first time.
This script does the following

  * Create ServiceConfiguration.Cloud.cscfg as a copy of configfiles/ServiceConfiguration.Cloud.cscfg.ref
  * Download mongodb binaries (currently 2.1.0-pre) to the appropriate solution location

## BUILDING

More details on building can be found at [build wiki](http://www.mongodb.org/display/DOCS/MongoDB+Replica+Sets+on+Azure#MongoDBReplicaSetsonAzure-Building)

### Prerequisites
  * .Net 4.0.
  * Visual Studio 2010 with SP1
    * Has been tested with Visual Web Developer Express and Visual Studio Ultimate
  * Windows Azure SDK 1.6 
  * Windows Azure Tools for Visual Studio 2010 1.6
  * MongoDB v2.1.0-pre- (downloaded through setup)
  * MongoDB C# driver v1.4-pre (embedded)

### Build
  * Open MongoDBReplicaSet.sln from Visual Studio and build

## RUNNING

More information can be found at the [configuration](http://www.mongodb.org/display/DOCS/Azure+ReplSet+Configuration) 
and [deployment](http://www.mongodb.org/display/DOCS/Azure+ReplSet+Deployment) wikis

### Default configuration
  * 3 replica set members
  * replica set name is rs
  * Local cache for data drive is 1GB
  * 512MB log directory

### Deploying and running

The following steps describe how to configure, deploy and run MongoDB replica sets on Azure alongwith the provided
sample app. More instructions on replacing the sample app with your own application can be found on the [wiki] 
(http://www.mongodb.org/display/DOCS/MongoDB+Replica+Sets+on+Azure)

### Running locally on compute/storage emulator
  * This should work out of the box and no special configuration is needed
  * The default configuration has
    * 3 replica set members running on ports 27017, 27018 and 27019
    * The default data directory size is 1GB (uses development storage)
    * Default log directory size is 512MB (local storage)
    * replica set nmae
  * In Visual Studio Run using F5 or Debug->Start Debugging
    * This will start the configured number of instances as part of the replica set and also the MvcMovie sample app
    * Note - Refer to the [troubleshooting](http://www.mongodb.org/display/DOCS/MongoDB+Replica+Sets+on+Azure#MongoDBReplicaSetsonAzure-FAQ%2FTroubleshooting) 
           section for debugging any failures

### Deploying to Azure
  * Create an Azure Storage account using the Azure Management Portal. You need the name and access
    keys to configure storage.
  * To deploy to the Azure cloud you need to edit the Cloud configuration and specify the cloud storage credentials
    from above
  * You can edit MongoDBReplicaSet\ServiceConfiguration.Cloud.cscfg or edit in Visual Studio
  * For ReplicaSetRole in the MongoDBReplicaSet cloud project, ensure the following are set
    * MongoDBDataDir - Set storage account credentials from above and use HTTP endpoints.
    * DiagnosticsConnectionString - Set storage account credentials from above and use HTTPS endpoints.
  * For MvcMovie role in the MongoDBReplicaSet cloud project, ensure the following are set
    * Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString - Set storage account credentials from above and 
                                                                  use HTTPS endpoints.

## Maintainers
* Sridhar Nanjundeswaran       sridhar@10gen.com

## MongoDB on Azure sample applications

## INITIAL SETUP
Run `samplesetup.cmd` to setup up the solution for building. This script only needs to be run the first time.
This script does the following

  * Create ServiceConfiguration.Cloud.cscfg as a copy of configfiles/ServiceConfiguration.Cloud.cscfg.ref
  * Download mongodb binaries (currently 2.1.0-pre) to the appropriate location

### Prerequisites
  * Windows x64
  * .Net 4.0.
  * Visual Studio 2010 with SP1
    * Has been tested with Visual Web Developer Express and Visual Studio Ultimate
  * Windows Azure SDK 1.6 
  * Windows Azure Tools for Visual Studio 2010 1.6

### Build
  * Open MongoDBReplicaSetMvcMovieSample.sln from Visual Studio and build

## RUNNING

### Deploying and running

### Running locally on compute/storage emulator
  * This should work out of the box and no special configuration is needed
  * The default configuration has
    * 3 replica set members running on ports 27017, 27018 and 27019
    * The default data directory size is 1GB (uses development storage)
    * Default log directory size is 512MB (local storage)
    * replica set name is rs
  * In Visual Studio Run using F5 or Debug->Start Debugging
    * This will start the configured number of instances as part of the replica set and also the MvcMovie sample app

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

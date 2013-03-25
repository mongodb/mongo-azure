## MongoDB on Azure Worker Roles

Welcome! This project allows you to easily deploy a MongoDB-based application to [Windows Azure](http://www.windowsazure.com/), particularly ones that use replica sets. (The demo project, MvcMovieSample, creates a three-node replica set by default.)

In this project, the <code>mongod</code> servers run as **separate worker role** instances. For information about running MongoDB on the newer IaaS virtual machines, see the [MongoDB Installer for Windows Azure](http://www.mongodb.org/display/DOCS/MongoDB+Installer+for+Windows+Azure).

## Quick Start

We assume you're running Windows 7 x64 and Visual Studio. If not, install those first; Visual Studio 2010 (or express) should work.

1. Install the [Windows Azure SDK](https://www.windowsazure.com/en-us/develop/net/) (we strongly recommend using June 2012).
2. Enable IIS on your local machine. This can be done by going to the "Turn Windows features on or off" control panel, under "Programs". Check "Internet Information Services" and also check ASP.NET under World Wide Web Services|Application Development Features.
3. Clone the project.
4. ***Before* opening either solution file**, run Setup\solutionsetup.cmd.
4. Open the solution you want, set the "MongoDB.WindowsAzure.[Sample.]Deploy" project as the StartUp Project, and run it!

The setup script does the following:
  * Creates the cloud configs for the 2 solutions ServiceConfiguration.Cloud.cscfg
  * Downloads the MongoDB binaries to lib\MongoDBBinaries.

**32-bit note:** The setup script downloads the 64-bit version of MongoDB by default. If you are developing with 32-bit Windows, you will need to download the latest 32-bit MongoDB binaries and place them in lib\MongoDBBinaries yourself. Do this after running solutionsetup.cmd so it won't overwrite your work.

## Documentation
http://www.mongodb.org/display/DOCS/MongoDB+on+Azure

## Directories
### lib
External libraries that are referenced by this project such as the MongoDB .Net Driver (http://www.mongodb.org/display/DOCS/CSharp+Language+Center)
### Setup
The initial run-once setup script
### src
The actual source files
### Tools
Related tools

## Prerequisites
  * Windows x64
  * .Net 4.0
  * Visual Studio 2010 with SP1
    * Has been tested with Visual Web Developer Express and Visual Studio Ultimate
  * [Windows Azure SDK and Tools - June 2012 Release](https://www.windowsazure.com/en-us/develop/net/)
  * For the sample application
    * IIS (for local testing)
    * ASP.Net Web Pages 1
    * ASP.Net MVC 3

## Solutions
  * MongoDB.WindowsAzure.sln - Solution containing just the MongoDB worker role
  * MongoDB.WindowsAzure.Sample.sln - Solution containing the MongoDB worker role and a sample Web role

## Running

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
  * Refer to documentation 

## Support, Mail Lists etc.
* http://www.mongodb.org/display/DOCS/Community

## Maintainer(s)
* Sridhar Nanjundeswaran    sridhar@10gen.com

## Contributors
* Phillip Cohen		    phillip.cohen@10gen.com 
* Stephen Steneker   stennie@10gen.com
* Gregor Macadam     gregor@10gen.com
* Alexander Nagy optimiz3@gmail.com

If you have contributed and we have neglected to add you to this list please contact one of the maintainers to be added to the list (with apologies).

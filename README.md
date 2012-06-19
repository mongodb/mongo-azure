## MongoDB on Azure Worker Roles

Welcome! This project allows you to deploy 

## Quick Start

We assume you're running Windows 7 x64 and Visual Studio. If not, install that first. 2008 or 2010 is supported by Azure.

1. Install the latest [Azure SDK](https://www.windowsazure.com/en-us/develop/net/) (we've used June 2012).
2. Enable IIS on your local machine. This can be done by going to the "Turn Windows features on or off" control panel, under "Programs". Check "Internet Information Services" and also check ASP.NET under World Wide Web Services|Application Development Features.
3. Clone the project.
4. **Before opening the solution files**, run Setup\solutionsetup.cmd.
4. Open either solution, set the "Deploy" project as the StartUp Project, and run it!

## Documentation
http://www.mongodb.org/display/DOCS/MongoDB+on+Azure

## Directories
### lib
External libraries that are referenced by this project such as the MongoDB .Net Driver (http://www.mongodb.org/display/DOCS/CSharp+Language+Center)
### Setup
One time project setup scripts
### src
The actual source files

## Prerequisites
  * Windows x64
  * .Net 4.0
  * Visual Studio 2010 with SP1
    * Has been tested with Visual Web Developer Express and Visual Studio Ultimate
  * Windows Azure SDK and Tools - June 2012 Release
  * For the sample application
    * IIS (for local testing)
    * ASP.Net Web Pages 1
    * ASP.Net MVC 3

## Solutions
  * MongoDB.WindowsAzure.sln - Solution containing just the MongoDB worker role
  * MongoDB.WindowsAzure.Sample.sln - Solution containing the MongoDB worker role and a sample Web role

## Initial Setup
Run `Setup\solutionsetup.cmd` to setup up the solutions for building. This script only needs to be run the first time.
This script does the following
  * Creates the cloud configs for the 2 solutions ServiceConfiguration.Cloud.cscfg
  * Download mongodb binaries to the appropriate solution location

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

## Maintainers
* Sridhar Nanjundeswaran    sridhar@10gen.com

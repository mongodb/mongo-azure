CheckinVerifier
===============

Verifies that configfuration files in the project are in sync, for use prior to a checkin.

**Instructions:** Build; run in the root level of mongo-azure.

Verifications
-------------

src\MongoDB.WindowsAzure.Deploy\ServiceDefinition.csdef is a proper subset of Samples\SampleMvcApp\SampleAzureDeploy\ServiceDefinition.csdef


src\MongoDB.WindowsAzure.Deploy\ServiceConfiguration.Local.cscfg is a proper subset of Samples\SampleMvcApp\SampleAzureDeploy\ServiceConfiguration.Local.cscfg


Setup\ServiceConfiguration.Cloud.cscfg.core is a proper subset of Setup\ServiceConfiguration.Cloud.cscfg.sample

CheckinVerifier
===============

Verifies that configfuration files in the project are in sync, for use prior to a checkin.

**Instructions:** Build; run in the root level of mongo-azure.

Verifications
-------------

**Source\AzureDeploy\ServiceDefinition.csdef** is a proper subset of **Samples\SampleMvcApp\SampleAzureDeploy\ServiceDefinition.csdef**


**Source\AzureDeploy\ServiceConfiguration.Local.cscfg** is a proper subset of **Samples\SampleMvcApp\SampleAzureDeploy\ServiceConfiguration.Local.cscfg**


**Source\Setup\ServiceConfiguration.Cloud.cscfg.ref** is a proper subset of **Samples\SampleMvcApp\Setup\ServiceConfiguration.Cloud.cscfg.ref**


**Source\Setup\ServiceConfiguration.Cloud.cscfg.ref** is the same as **Source\AzureDeploy\ServiceConfiguration.Local.cscfg except for values of ConnectionStrings**


**Source\Setup\ServiceConfiguration.Cloud.cscfg.ref** is the same as **Source\AzureDeploy\ServiceConfiguration.Cloud.cscfg except for values of ConnectionStrings**
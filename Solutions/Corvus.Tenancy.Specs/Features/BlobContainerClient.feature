@perFeatureContainer
@setupTenantedBlobContainerClient

Feature: BlobContainerClient
	In order to use cloud blob storage for tenanted services
	As a developer
	I want to be able to manage the container

Scenario: Create a container without overriding any settings in the blob storage configuration
	Given I have added blob storage configuration to the current tenant with a table name of 'newname'
	Then I should be able to get the tenanted cloud blob container


# IDG TODO:
# Verify that the new helpers in Corvus.Azure.Storage.Tenancy's ContainerNameBuilders make it possible to
# access data that was originally stored using the old automatic container creation. The tests need to create
# a container with the same naming scheme that would originally have been created automatically, and then
# demonstrate that we can access it using the new C3 configuration approach.
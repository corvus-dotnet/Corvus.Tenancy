@setupContainer
@setupTenantedCloudBlobContainer

Feature: CloudBlobContainer
	In order to use cloud blob storage for tenanted services
	As a developer
	I want to be able to manage the container


Scenario: Create a container
	Then I should be able to get the tenanted cloud blob container

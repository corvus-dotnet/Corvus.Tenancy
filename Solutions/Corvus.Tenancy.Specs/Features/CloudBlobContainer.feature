@setupContainer
@setupTenantedCloudBlobContainer

Feature: CloudBlobContainer
	In order to use cloud blob storage for tenanted services
	As a developer
	I want to be able to manage the container

Scenario: Create a container without overriding any settings in the blob storage configuration
	Given I have added blob storage configuration to the current tenant
	| Container | DisableTenantIdPrefix |
	|           | false                 |
	Then I should be able to get the tenanted cloud blob container
	And the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob container definition

Scenario: Create a container with a name specified in the blob storage configuration
	Given I have added blob storage configuration to the current tenant
	| Container | DisableTenantIdPrefix |
	| newname   | false                 |
	Then I should be able to get the tenanted cloud blob container
	And the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob configuration

Scenario: Create a container with a name specified in the blob storage configuration and without the tenant Id prefix
	Given I have added blob storage configuration to the current tenant
	| Container | DisableTenantIdPrefix |
	| newname   | true                  |
	Then I should be able to get the tenanted cloud blob container
	And the tenanted cloud blob container should be named using a hash of the name specified in the blob configuration

Scenario: Remove configuration from tenant
	Given I have added blob storage configuration to the current tenant
	| Container | DisableTenantIdPrefix |
	|           | false                 |
	When I remove the blob storage configuration from the tenant
	Then attempting to get the blob storage configuration from the tenant throws an ArgumentException
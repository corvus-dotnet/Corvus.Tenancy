@perFeatureContainer
@setupTenantedCloudTable

Feature: CloudTable
	In order to use cloud table storage for tenanted services
	As a developer
	I want to be able to manage the table

Scenario: Create a table
	Given I have added table storage configuration to the current tenant with a table name of 'newname'
	Then I should be able to get the tenanted cloud table

Scenario: Remove configuration from tenant
	Given I have added table storage configuration to the current tenant with a table name of 'newname'
	When I remove the table storage configuration from the tenant
	Then attempting to get the table storage configuration from the tenant throws an ArgumentException
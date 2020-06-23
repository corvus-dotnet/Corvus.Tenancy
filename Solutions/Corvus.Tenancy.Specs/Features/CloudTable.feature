@setupContainer
@setupTenantedCloudTable

Feature: CloudTable
	In order to use cloud table storage for tenanted services
	As a developer
	I want to be able to manage the table


Scenario: Create a table
	Then I should be able to get the tenanted cloud table

Scenario: Remove configuration from tenant
	When I remove the table storage configuration from the tenant
	Then attempting to get the table storage configuration from the tenant throws an ArgumentException
@perScenarioContainer
@setupTenantedCloudTable

Feature: CloudTable
	In order to use cloud table storage for tenanted services
	As a developer
	I want to be able to manage the table

Scenario: Create a table without overriding any settings in the table configuration
	Given I have added table storage configuration to the current tenant
	| TableName | DisableTenantIdPrefix |
	|           | false                 |
	Then I should be able to get the tenanted cloud table
	And the tenanted cloud table should be named using a hash of the tenant Id and the name specified in the table definition

Scenario: Create a table with a name specified in the table configuration
	Given I have added table storage configuration to the current tenant
	| TableName | DisableTenantIdPrefix |
	| newname   | false                 |
	Then I should be able to get the tenanted cloud table
	And the tenanted cloud table should be named using a hash of the tenant Id and the name specified in the table configuration

Scenario: Create a table with a name specified in the table configuration and without the tenant Id prefix
	Given I have added table storage configuration to the current tenant
	| TableName | DisableTenantIdPrefix |
	| newname   | true                  |
	Then I should be able to get the tenanted cloud table
	And the tenanted cloud table should be named using a hash of the name specified in the blob configuration

Scenario: Remove configuration from tenant
	Given I have added table storage configuration to the current tenant
	| TableName | DisableTenantIdPrefix |
	|           | false                 |
	When I remove the table storage configuration from the tenant
	Then attempting to get the table storage configuration from the tenant throws an ArgumentException
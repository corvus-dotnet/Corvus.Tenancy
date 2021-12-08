@perScenarioContainer
@setupLegacyTenantedCosmosContainer

Feature: Legacy CosmosContainer
	In order to use cosmos storage for tenanted services
	As a developer
	I want to be able to manage the container


Scenario: Create a cosmos container
    Given I have added legacy Cosmos configuration to a tenant
	Then I should be able to get the tenanted cosmos container from the legacy API

Scenario: Remove configuration from tenant
	When I remove the legacy Cosmos configuration from the tenant
	Then attempting to get the legacy Cosmos configuration from the tenant throws an ArgumentException
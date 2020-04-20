@setupContainer
@setupTenantedCosmosContainer

Feature: CosmosContainer
	In order to use cosmos storage for tenanted services
	As a developer
	I want to be able to manage the container


Scenario: Create a cosmos container
	Then I should be able to get the tenanted cosmos container

Scenario: Remove configuration from tenant
	When I remove the Cosmos configuration from the tenant
	Then attempting to get the Cosmos configuration from the tenant throws an ArgumentException
@setupContainer
@setupTenantedCosmosContainer

Feature: CosmosContainer
	In order to use cosmos storage for tenanted services
	As a developer
	I want to be able to manage the container


Scenario: Create a cosmos container
	Then I should be able to get the tenanted cosmos container

@perScenarioContainer
@setupTenantedCosmosContainer

Feature: CosmosContainer
    In order to use cosmos storage for tenanted services
    As a developer
    I want to be able to manage the container


Scenario: Get a cosmos container
    Given I have added Cosmos configuration to a tenant
    Then I should be able to get the tenanted cosmos container
    And the tenanted cosmos container database should match the configuration
    And the tenanted cosmos container name should match the configuration

Scenario: Remove configuration from tenant
    Given I have added Cosmos configuration to a tenant
    When I remove the cosmos configuration to a tenant
	Then attempting to get the Cosmos configuration from the tenant throws an InvalidOperationException

Scenario: Remove non-existent configuration from tenant
    Given I have not added cosmos configuration to a tenant
	Then attempting to get the Cosmos configuration from the tenant throws an InvalidOperationException
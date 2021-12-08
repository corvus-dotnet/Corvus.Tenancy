@perScenarioContainer
@setupTenantedGremlinClient

Feature: GremlinContainer
	In order to use cosmos storage via Gremlin for tenanted services
	As a developer
	I want to be able to manage the container


Scenario: Create a gremlin client
    Given I have added Gremlin configuration to a tenant
	Then I should be able to get the tenanted gremlin client

Scenario: Remove configuration from tenant
    Given I have not added Gremlin configuration to a tenant
	When I remove the Gremlin configuration from the tenant
	Then attempting to get the Gremlin configuration from the tenant should throw an ArgumentException
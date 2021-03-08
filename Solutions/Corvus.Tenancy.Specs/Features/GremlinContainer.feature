@perFeatureContainer
@setupTenantedGremlinClient

Feature: GremlinContainer
	In order to use cosmos storage via Gremlin for tenanted services
	As a developer
	I want to be able to manage the container


Scenario: Create a gremlin client
	Then I should be able to get the tenanted gremlin client

Scenario: Remove configuration from tenant
	When I remove the Gremlin configuration from the tenant
	Then attempting to get the Gremlin configuration from the tenant throws an ArgumentException
@setupContainer
@setupRecreatableTenantedCosmosContainer

Feature: RecreatableCosmosContainer
	In order to use cosmos storage for tenanted services
	As a developer
	I want to be able to recreate the container to support scenarios such as config refresh and key rotation.

Scenario: Recreate the cosmos container
	Given I get the recreatable tenanted cosmos container as "Original"
	When I recreate the cosmos container as "Recreated"
	Then the "Recreated" container should not be null


Feature: TenantCloudBlobContainerFactorySetup
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Initialise with no default configuration
	Given I do not have default configuration for tenanted blob storage
	When I add the tenant cloud blob container factory to my service collection using the provided extension method
	Then no default storage configuration is added to the root tenant

Scenario: Initialise with default configuration that does not contain an account name
	Given I have default configuration for tenanted blob storage that uses the storage emulator
	When I add the tenant cloud blob container factory to my service collection using the provided extension method
	Then the default storage configuration is added to the root tenant

Scenario: Initialise with default configuration that does contain an account name
	Given I have default configuration for tenanted blob storage that uses a real storage account
	When I add the tenant cloud blob container factory to my service collection using the provided extension method
	Then the default storage configuration is added to the root tenant

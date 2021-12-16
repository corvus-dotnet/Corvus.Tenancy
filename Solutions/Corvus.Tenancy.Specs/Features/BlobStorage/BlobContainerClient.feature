@perScenarioContainer

Feature: BlobContainerClient
    In order to use Azure Blob Storage for tenanted services
    As a developer
    I want to be able to obtain suitably-configured BlobContainerClient instances

Scenario: All details in configuration
    Given I have added blob storage configuration a tenant with the a container name of 'configuredcontainer'
    When I get a BlobContainerClient for the tenant without specifying a container name
    Then the BlobContainerClient source should have been given a configuration identical to the original one

Scenario: Container name not in configuration
    Given I have added blob storage configuration a tenant without a container name
    When I get a BlobContainerClient for the tenant specifying a container name of 'dynamiccontainer'
    Then the BlobContainerClient source should have been given a configuration based on the original one but with the Container set to 'dynamiccontainer'

Scenario: Container name in configuration overridden
    Given I have added blob storage configuration a tenant with the a container name of 'configuredcontainer'
    When I get a BlobContainerClient for the tenant specifying a container name of 'dynamiccontainer'
    Then the BlobContainerClient source should have been given a configuration based on the original one but with the Container set to 'dynamiccontainer'

Scenario: Remove configuration from tenant
    Given I have added blob storage configuration a tenant with the a container name of 'ConfiguredContainer'
	When I remove the blob storage configuration from the tenant
	Then attempting to get the blob storage configuration from the tenant throws an ArgumentException

# These next two are typically used in key rotation scenarios

Scenario: Get replacement when container goes bad details in configuration
    Given I have added blob storage configuration a tenant with the a container name of 'configuredcontainer'
    And I get a BlobContainerClient for the tenant without specifying a container name
    When I get a replacement BlobContainerClient for the tenant without specifying a container name
    Then the BlobContainerClient source should have been asked to replace a configuration identical to the original one

Scenario: Get replacement when container goes bad name not in configuration
    Given I have added blob storage configuration a tenant without a container name
    And I get a BlobContainerClient for the tenant specifying a container name of 'dynamiccontainer'
    When I get a replacement BlobContainerClient for the tenant specifying a container name of 'dynamiccontainer'
    Then the BlobContainerClient source should have been asked to replace a configuration based on the original one but with the Container set to 'dynamiccontainer'

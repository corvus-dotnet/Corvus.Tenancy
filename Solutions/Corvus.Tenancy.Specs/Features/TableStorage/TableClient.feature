@perScenarioTable

Feature: TableClient
    In order to use Azure Table Storage for tenanted services
    As a developer
    I want to be able to obtain suitably-configured TableClient instances

Scenario: All details in configuration
    Given I have added table storage configuration to a tenant with a table name of 'configuredtable'
    When I get a TableClient for the tenant without specifying a table name
    Then the TableClient source should have been given a configuration identical to the original one

Scenario: Table name not in configuration
    Given I have added table storage configuration to a tenant without a table name
    When I get a TableClient for the tenant specifying a table name of 'dynamictable'
    Then the TableClient source should have been given a configuration based on the original one but with the table name set to 'dynamictable'

Scenario: Table name in configuration overridden
    Given I have added table storage configuration to a tenant with a table name of 'configuredtable'
    When I get a TableClient for the tenant specifying a table name of 'dynamictable'
    Then the TableClient source should have been given a configuration based on the original one but with the table name set to 'dynamictable'

Scenario: Remove configuration from tenant
    Given I have added table storage configuration to a tenant with a table name of 'ConfiguredTable'
	When I remove the table storage configuration from the tenant
	Then attempting to get the table storage configuration from the tenant throws an InvalidOperationException

# These next two are typically used in key rotation scenarios

Scenario: Get replacement when table goes bad details in configuration
    Given I have added table storage configuration to a tenant with a table name of 'configuredtable'
    And I get a TableClient for the tenant without specifying a table name
    When I get a replacement TableClient for the tenant without specifying a table name
    Then the TableClient source should have been asked to replace a configuration identical to the original one

Scenario: Get replacement when table goes bad name not in configuration
    Given I have added table storage configuration to a tenant without a table name
    And I get a TableClient for the tenant specifying a table name of 'dynamictable'
    When I get a replacement TableClient for the tenant specifying a table name of 'dynamictable'
    Then the TableClient source should have been asked to replace a configuration based on the original one but with the table name set to 'dynamictable'

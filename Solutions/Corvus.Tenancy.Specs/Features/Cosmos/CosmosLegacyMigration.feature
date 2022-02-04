@perScenarioContainer
@setupTenantedCosmosContainer

Feature: CosmosLegacyMigration
    In order to migrate from v2 to v3 of Corvus.Tenancy when using Cosmos DB for tenanted services
    As a developer using Corvus.Storage.Azure.Cosmos.Tenancy
    I need to be able to work with storage accounts and tenant configurations created through the v2 libraries when using the v3 libraries


# --- V2 config only ---

# Neither Database nor Container exist yet

Scenario Outline: Database does not yet exist and only v2 configuration exists in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of <DatabaseThroughput> and container throughput of <ContainerThroughput>
    Then a new Cosmos database with the specified name should have been created
    And the Cosmos database throughput should match the specified <DatabaseThroughput>
    And a new Cosmos container with the specified name should have been created
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    And the Cosmos Container object returned should refer to the database
    And the Cosmos Container object returned should refer to the container

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames |
    | Config                   | 400                |                     | false                   |
    | Config                   |                    | 400                 | false                   |
    | Config                   | 400                | 500                 | false                   |
    | Config                   | 400                |                     | true                    |
    | Config                   |                    | 400                 | true                    |
    | Config                   | 400                | 500                 | true                    |
    | Args                     | 400                |                     | true                    |
    | Args                     |                    | 400                 | true                    |
    | Args                     | 400                | 500                 | true                    |
    | DbInConfigContainerInArg | 400                |                     | true                    |
    | DbInConfigContainerInArg |                    | 400                 | true                    |
    | DbInConfigContainerInArg | 400                | 500                 | true                    |

Scenario Outline: Database does not yet exist and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of <DatabaseThroughput> and container throughput of <ContainerThroughput>
    Then a new Cosmos database with the specified name should have been created
    And the Cosmos database throughput should match the specified <DatabaseThroughput>
    And a new Cosmos container with the specified name should have been created
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    And MigrateToV3Async should have returned a CosmosContainerConfiguration with settings matching the legacy CosmosConfiguration

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames |
    | Config                   | 400                |                     | false                   |
    | Config                   |                    | 400                 | false                   |
    | Config                   | 400                | 500                 | false                   |
    | Config                   | 400                |                     | true                    |
    | Config                   |                    | 400                 | true                    |
    | Config                   | 400                | 500                 | true                    |
    | Args                     | 400                |                     | true                    |
    | Args                     |                    | 400                 | true                    |
    | Args                     | 400                | 500                 | true                    |
    | DbInConfigContainerInArg | 400                |                     | true                    |
    | DbInConfigContainerInArg |                    | 400                 | true                    |
    | DbInConfigContainerInArg | 400                | 500                 | true                    |


# Database exists, container doesn't yet.

Scenario Outline: Database exists but container does not yet exist and only v2 configuration exists in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database already exists with throughput of <DatabaseThroughput>
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of <DatabaseThroughput> and container throughput of <ContainerThroughput>
    Then a new Cosmos container with the specified name should have been created
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    And the Cosmos Container object returned should refer to the database
    And the Cosmos Container object returned should refer to the container

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames |
    | Config                   | 400                |                     | false                   |
    | Config                   |                    | 400                 | false                   |
    | Config                   | 400                | 500                 | false                   |
    | Config                   | 400                |                     | true                    |
    | Config                   |                    | 400                 | true                    |
    | Config                   | 400                | 500                 | true                    |
    | Args                     | 400                |                     | true                    |
    | Args                     |                    | 400                 | true                    |
    | Args                     | 400                | 500                 | true                    |
    | DbInConfigContainerInArg | 400                |                     | true                    |
    | DbInConfigContainerInArg |                    | 400                 | true                    |
    | DbInConfigContainerInArg | 400                | 500                 | true                    |

Scenario Outline: Database exists but container does not yet exist and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database already exists with throughput of <DatabaseThroughput>
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of <DatabaseThroughput> and container throughput of <ContainerThroughput>
    Then a new Cosmos container with the specified name should have been created
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    And MigrateToV3Async should have returned a CosmosContainerConfiguration with settings matching the legacy CosmosConfiguration

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames |
    | Config                   | 400                |                     | false                   |
    | Config                   |                    | 400                 | false                   |
    | Config                   | 400                | 500                 | false                   |
    | Config                   | 400                |                     | true                    |
    | Config                   |                    | 400                 | true                    |
    | Config                   | 400                | 500                 | true                    |
    | Args                     | 400                |                     | true                    |
    | Args                     |                    | 400                 | true                    |
    | Args                     | 400                | 500                 | true                    |
    | DbInConfigContainerInArg | 400                |                     | true                    |
    | DbInConfigContainerInArg |                    | 400                 | true                    |
    | DbInConfigContainerInArg | 400                | 500                 | true                    |


# Database and container exist already.

Scenario Outline: Container exists and only v2 configuration exists in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database already exists with throughput of 400
    And the Cosmos container already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then the Cosmos Container object returned should refer to the database
    And the Cosmos Container object returned should refer to the container

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | Args                     | true                    |
    | DbInConfigContainerInArg | true                    |

Scenario: Container exists and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database already exists with throughput of 400
    And the Cosmos container already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then MigrateToV3Async should have returned a CosmosContainerConfiguration with settings matching the legacy CosmosConfiguration

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | Args                     | true                    |
    | DbInConfigContainerInArg | true                    |

# --- V2 and V3 config present ---

Scenario: Container exists and both v2 and v3 configurations exist in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to a bogus legacy CosmosConfiguration
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database already exists with throughput of 400
    And the Cosmos container already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of 42 and container throughput of 99
    Then the Cosmos Container object returned should refer to the database
    And the Cosmos Container object returned should refer to the container

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | Args                     | true                    |
    | DbInConfigContainerInArg | true                    |

Scenario: Container exists and both v2 and v3 configurations exist in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to a bogus legacy CosmosConfiguration
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database already exists with throughput of 400
    And the Cosmos container already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then MigrateToV3Async should have returned null

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | Args                     | true                    |
    | DbInConfigContainerInArg | true                    |


# --- only V3 config present ---

Scenario: Container exists and only v3 configurations exist in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database already exists with throughput of 400
    And the Cosmos container already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then the Cosmos Container object returned should refer to the database
    And the Cosmos Container object returned should refer to the container

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | Args                     | true                    |
    | DbInConfigContainerInArg | true                    |

Scenario: Container exists and only v3 configurations exist in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to a bogus legacy CosmosConfiguration
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database already exists with throughput of 400
    And the Cosmos container already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | Args                     | true                    |
    | DbInConfigContainerInArg | true                    |

# TODO: neither v2 nor v3 present (for both GetContainerForTenantAsync and MigrateToV3Async)
# Also: integration tests in which we actually write data out with the V2 code and read it back in with the V3 code?
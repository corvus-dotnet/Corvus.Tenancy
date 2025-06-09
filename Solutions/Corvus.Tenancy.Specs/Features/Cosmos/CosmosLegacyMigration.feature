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
    Then a new Cosmos database and container with names from '<DbNameExpectedFrom>' and '<ContainerNameExpectedFrom>' should have been created
    And the Cosmos database throughput should match the specified <DatabaseThroughput>
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    Then the Cosmos Container object returned should refer to the database with the name from '<DbNameExpectedFrom>'
    And the Cosmos Container object returned should refer to the container with the name from '<ContainerNameExpectedFrom>'

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames | DbNameExpectedFrom  | ContainerNameExpectedFrom |
    | Config                   | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Args                     | 400                |                     | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     |                    | 400                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     | 400                | 500                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | ConfigAndArgs            | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | DbInConfigContainerInArg | 400                |                     | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg |                    | 400                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                | 500                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                |                     | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg |                    | 400                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg | 400                | 500                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |

Scenario Outline: Database does not yet exist and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of <DatabaseThroughput> and container throughput of <ContainerThroughput>
    Then a new Cosmos database and container with names from '<DbNameExpectedFrom>' and '<ContainerNameExpectedFrom>' should have been created
    And the Cosmos database throughput should match the specified <DatabaseThroughput>
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    And MigrateToV3Async should have returned a CosmosContainerConfiguration with settings matching the legacy CosmosConfiguration

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames | DbNameExpectedFrom  | ContainerNameExpectedFrom |
    | Config                   | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Args                     | 400                |                     | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     |                    | 400                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     | 400                | 500                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | ConfigAndArgs            | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | DbInConfigContainerInArg | 400                |                     | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg |                    | 400                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                | 500                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                |                     | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg |                    | 400                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg | 400                | 500                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |


# Database exists, container doesn't yet.

Scenario Outline: Database exists but container does not yet exist and only v2 configuration exists in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database specified in v2 configuration already exists with throughput of <DatabaseThroughput>
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of <DatabaseThroughput> and container throughput of <ContainerThroughput>
    Then under the db with the name from '<DbNameExpectedFrom>' a new container with the name from '<ContainerNameExpectedFrom>' should have been created
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    Then the Cosmos Container object returned should refer to the database with the name from '<DbNameExpectedFrom>'
    And the Cosmos Container object returned should refer to the container with the name from '<ContainerNameExpectedFrom>'

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames | DbNameExpectedFrom  | ContainerNameExpectedFrom |
    | Config                   | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Args                     | 400                |                     | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     |                    | 400                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     | 400                | 500                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | ConfigAndArgs            | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | DbInConfigContainerInArg | 400                |                     | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg |                    | 400                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                | 500                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                |                     | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg |                    | 400                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg | 400                | 500                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |

Scenario Outline: Database exists but container does not yet exist and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database specified in v2 configuration already exists with throughput of <DatabaseThroughput>
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of <DatabaseThroughput> and container throughput of <ContainerThroughput>
    Then under the db with the name from '<DbNameExpectedFrom>' a new container with the name from '<ContainerNameExpectedFrom>' should have been created
    And the Cosmos container throughput should match the specified <ContainerThroughput>
    And MigrateToV3Async should have returned a CosmosContainerConfiguration with settings matching the legacy CosmosConfiguration

    Examples:
    | Namelocations            | DatabaseThroughput | ContainerThroughput | AutoTenantSpecificNames | DbNameExpectedFrom  | ContainerNameExpectedFrom |
    | Config                   | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Config                   | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Args                     | 400                |                     | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     |                    | 400                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | Args                     | 400                | 500                 | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | ConfigAndArgs            | 400                |                     | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            |                    | 400                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                | 500                 | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | 400                |                     | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            |                    | 400                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | ConfigAndArgs            | 400                | 500                 | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | DbInConfigContainerInArg | 400                |                     | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg |                    | 400                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                | 500                 | false                   | V2ConfigExact       | LogicalNameExact          |
    | DbInConfigContainerInArg | 400                |                     | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg |                    | 400                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |
    | DbInConfigContainerInArg | 400                | 500                 | true                    | V2ConfigTenanted    | LogicalNameTenanted       |


# Database and container exist already.

Scenario Outline: Container exists and only v2 configuration exists in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database specified in v2 configuration already exists with throughput of 400
    And the Cosmos container specified in v2 configuration already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then the Cosmos Container object returned should refer to the database with the name from '<DbNameExpectedFrom>'
    And the Cosmos Container object returned should refer to the container with the name from '<ContainerNameExpectedFrom>'

    Examples:
    | Namelocations            | AutoTenantSpecificNames | DbNameExpectedFrom  | ContainerNameExpectedFrom |
    | Config                   | false                   | V2ConfigExact       | V2ConfigExact             |
    | Config                   | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | Args                     | true                    | LogicalNameTenanted | LogicalNameTenanted       |
    | ConfigAndArgs            | false                   | V2ConfigExact       | V2ConfigExact             |
    | ConfigAndArgs            | true                    | V2ConfigTenanted    | V2ConfigTenanted          |
    | DbInConfigContainerInArg | true                    | V2ConfigTenanted    | LogicalNameTenanted       |

Scenario: Container exists and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to the legacy CosmosConfiguration
    And the Cosmos database specified in v2 configuration already exists with throughput of 400
    And the Cosmos container specified in v2 configuration already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then MigrateToV3Async should have returned a CosmosContainerConfiguration with settings matching the legacy CosmosConfiguration

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | Args                     | true                    |
    | ConfigAndArgs            | false                   |
    | ConfigAndArgs            | true                    |
    | DbInConfigContainerInArg | true                    |

# --- V2 and V3 config present ---

Scenario: Container exists and both v2 and v3 configurations exist in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to a bogus legacy CosmosConfiguration
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database specified in v3 configuration already exists with throughput of 400
    And the Cosmos container specified in v3 configuration already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of 42 and container throughput of 99
    Then the Cosmos Container object returned should refer to the database with the name from '<DbNameExpectedFrom>'
    And the Cosmos Container object returned should refer to the container with the name from '<ContainerNameExpectedFrom>'

    Examples:
    | Namelocations            | AutoTenantSpecificNames | DbNameExpectedFrom | ContainerNameExpectedFrom |
    | Config                   | false                   | V3ConfigExact      | V3ConfigExact             |
    | Config                   | true                    | V3ConfigExact      | V3ConfigExact             |
    | ConfigAndArgs            | false                   | V3ConfigExact      | V3ConfigExact             |
    | ConfigAndArgs            | true                    | V3ConfigExact      | V3ConfigExact             |
    | DbInConfigContainerInArg | false                   | V3ConfigExact      | LogicalNameExact          |
    | DbInConfigContainerInArg | true                    | V3ConfigExact      | LogicalNameExact          | # Exact because auto-tenanting can only be configured for V2

Scenario: Container exists and both v2 and v3 configurations exist in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to a bogus legacy CosmosConfiguration
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database specified in v3 configuration already exists with throughput of 400
    And the Cosmos container specified in v3 configuration already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | ConfigAndArgs            | false                   |
    | ConfigAndArgs            | true                    |
    | DbInConfigContainerInArg | true                    |


# --- only V3 config present ---

Scenario: Container exists and only v3 configurations exist in tenant properties when container requested
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database specified in v3 configuration already exists with throughput of 400
    And the Cosmos container specified in v3 configuration already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then the Cosmos Container object returned should refer to the database with the name from 'V3ConfigExact'
    And the Cosmos Container object returned should refer to the container with the name from '<ContainerNameExpectedFrom>'

    Examples:
    | Namelocations            | AutoTenantSpecificNames | ContainerNameExpectedFrom |
    | Config                   | false                   | V3ConfigExact             |
    | Config                   | true                    | V3ConfigExact             |
    | ConfigAndArgs            | false                   | V3ConfigExact             |
    | ConfigAndArgs            | true                    | V3ConfigExact             |
    | DbInConfigContainerInArg | true                    | LogicalNameExact          |

Scenario: Container exists and only v3 configurations exist in tenant properties when configuration migration preparation occurs
    Given Cosmos database and container names set in <Namelocations> with tenant-specific names set to '<AutoTenantSpecificNames>'
    And the tenant has the property 'sv2' set to a bogus legacy CosmosConfiguration
    And the tenant has the property 'sv3' set to the CosmosContainerConfiguration
    And the Cosmos database specified in v3 configuration already exists with throughput of 400
    And the Cosmos container specified in v3 configuration already exists with per-database throughput
    When ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3' with db throughput of 99 and container throughput of 42
    Then ICosmosContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null

    Examples:
    | Namelocations            | AutoTenantSpecificNames |
    | Config                   | false                   |
    | Config                   | true                    |
    | ConfigAndArgs            | false                   |
    | ConfigAndArgs            | true                    |
    | DbInConfigContainerInArg | true                    |

# TODO: neither v2 nor v3 present (for both GetContainerForTenantAsync and MigrateToV3Async)
# Also: integration tests in which we actually write data out with the V2 code and read it back in with the V3 code?
@perScenarioContainer
@tableStorageLegacyMigration

Feature: TableLegacyMigration
    In order to migrate from v2 to v3 of Corvus.Tenancy when using table storage for tenanted services
    As a developer using Corvus.Storage.Azure.TaStorage.Tenancy
    I need to be able to work with storage accounts and tenant configurations created through the v2 libraries when using the v3 libraries

# --- V2 config, Table doesn't exist yet ---

Scenario Outline: Table does not yet exist and only v2 configuration with TableName exists in tenant properties when table requested
    Given a legacy TableConfiguration with an AccountName and a TableName with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with a name derived from the legacy configuration TableName
    And a new table with a name derived from the legacy configuration TableName should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario: Table does not yet exist and only v2 configuration without TableName exists in tenant properties when table requested
    Given a legacy TableConfiguration with an AccountName
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with a name derived from the logical table name
    And a new table with a name derived from the logical table name should have been created

Scenario Outline: Table does not yet exist and only v2 configuration with TableName exists in tenant properties when configuration migration preparation occurs
    Given a legacy TableConfiguration with an AccountName and a TableName with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a TableConfiguration with these settings
    | ConnectionStringPlainText   | TableName             |
    | testAccountConnectionString | DerivedFromConfigured |
    And a new table with a name derived from the legacy configuration TableName should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario: Table does not yet exist and only v2 configuration without TableName exists in tenant properties when configuration migration preparation occurs
    Given a legacy TableConfiguration with an AccountName
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a TableConfiguration with these settings
    | ConnectionStringPlainText   | TableName             |
    | testAccountConnectionString | DerivedFromLogical |
    And a new table with a name derived from the logical table name should have been created

# TODO: multiple tables

# --- V2 config, Table does exist ---

Scenario Outline: Table exists and only v2 configuration with TableName exists in tenant properties when table requested
    Given a legacy TableConfiguration with an AccountName and a TableName with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a table with a tenant-specific name derived from the configured TableName exists
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with a name derived from the legacy configuration TableName
    And no new table should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario Outline: Table exists and only v2 configuration without TableName exists in tenant properties when table requested
    Given a legacy TableConfiguration with an AccountName
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a table with a name derived from the logical table name exists
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with a name derived from the logical table name
    And no new table should have been created

# This covers two scenarios:
#   The table was created through normal use of the v2 libraries before we upgraded to v3
#   We manage to create the new table and then crash before storing the relevant new-form
#       configuration in the tenant
Scenario Outline: Table exists and only v2 configuration with TableName exists in tenant properties when configuration migration preparation occurs
    # AccountName is interpreted as a connection string when there's no AccountKeySecretName
    Given a legacy TableConfiguration with an AccountName and a TableName with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a table with a tenant-specific name derived from the configured TableName exists
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a TableConfiguration with these settings
    | ConnectionStringPlainText   | TableName             |
    | testAccountConnectionString | DerivedFromConfigured |
    And no new table should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario: Table exists and only v2 configuration without TableName exists in tenant properties when configuration migration preparation occurs
    # AccountName is interpreted as a connection string when there's no AccountKeySecretName
    Given a legacy TableConfiguration with an AccountName
    And this test is using an Azure TableClient with a connection string
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a table with a name derived from the logical table name exists
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a TableConfiguration with these settings
    | ConnectionStringPlainText   | TableName          |
    | testAccountConnectionString | DerivedFromLogical |
    And no new table should have been created


# --- V2 and V3 config (Table exists, because it always will if V3 config is present) ---

# TODO: Table name in config vs table name in argument
Scenario Outline: Table exists and both v2 and v3 configurations with TableName exist in tenant properties when table requested
    Given a legacy TableConfiguration with a bogus AccountName and a TableName with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText and a TableName
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with the name in the V3 configuration exists
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with the name in the V3 configuration
    And no new table should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

    # !!! v3 with/without table!?
Scenario: Table exists and both v2 and v3 configurations without TableName exist in tenant properties when table requested
    Given a legacy TableConfiguration with a bogus AccountName
    And this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with a name derived from the logical table name exists
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with a name derived from the logical table name
    And no new table should have been created

Scenario Outline: Table exists and both v2 configuration without table and v3 configuration with TableName exist in tenant properties when table requested
    Given a legacy TableConfiguration with a bogus AccountName and a TableName with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText and a TableName
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with the name in the V3 configuration exists
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with the name in the V3 configuration
    And no new table should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario: Table exists and both v2 and v3 configurations with TableName exist in tenant properties when configuration migration preparation occurs
    Given a legacy TableConfiguration with a bogus AccountName and a TableName with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText and a TableName
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with the name in the V3 configuration exists
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new table should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario: Table exists and both v2 and v3 configurations without TableName exist in tenant properties when configuration migration preparation occurs
    Given a legacy TableConfiguration with a bogus AccountName
    And this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText and a TableName
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with a name derived from the logical table name exists
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new table should have been created

Scenario: Table exists and both v2 configuration without table and v3 configuration with TableName exist in tenant properties when configuration migration preparation occurs
    Given a legacy TableConfiguration with a bogus AccountName
    And this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText and a TableName
    And a tenant with the property 'sv2' set to the legacy TableConfiguration
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with the name in the V3 configuration exists
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new table should have been created


# --- V3 config only (Table exists, because it always will if V3 config is present) ---

Scenario: Table exists and only v3 configuration with TableName exists in tenant properties when table requested
    Given this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText and a TableName
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with the name in the V3 configuration exists
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with the name in the V3 configuration
    And no new table should have been created

Scenario: Table exists and only v3 configuration without TableName exists in tenant properties when table requested
    Given this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with a name derived from the logical table name exists
    When ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the TableClient should have access to the table with a name derived from the logical table name
    And no new table should have been created

Scenario: Table exists and only v3 configuration with TableName exists in tenant properties when configuration migration preparation occurs
    Given this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText and a TableName
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with the name in the V3 configuration exists
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new table should have been created

Scenario: Table exists and only v3 configuration without TableName exists in tenant properties when configuration migration preparation occurs
    Given this test is using an Azure TableClient with a connection string
    And a v3 TableConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv3' set to the v3 TableConfiguration
    And a table with a name derived from the logical table name exists
    When ITableSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then ITableSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new table should have been created

# TODO: neither v2 nor v3 present (for both GetTableClientFromTenantAsync and MigrateToV3Async)
# Also: integration tests in which we actually write data out with the V2 code and read it back in with the V3 code?
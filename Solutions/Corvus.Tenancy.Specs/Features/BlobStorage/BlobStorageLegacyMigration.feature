@perScenarioContainer
@blobStorageLegacyMigration

Feature: BlobStorageLegacyMigration
    In order to migrate from v2 to v3 of Corvus.Tenancy when using cloud blob storage for tenanted services
    As a developer using Corvus.Storage.Azure.BlobStorage.Tenancy
    I need to be able to work with storage accounts and tenant configurations created through the v2 libraries when using the v3 libraries

# --- V2 config, Container doesn't exist yet ---

Scenario Outline: Container does not yet exist and only v2 configuration with Container and tenant prefix enabled exists in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>' and a Container name with DisableTenantIdPrefix of false
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a tenanted name derived from the legacy configuration Container
    And a new container with a tenanted name derived from the legacy configuration Container should have been created with public access of '<ExpectedAccessType>'

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container does not yet exist and only v2 configuration with Container and tenant prefix disabled exists in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>' and a Container name with DisableTenantIdPrefix of true
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a non-tenant-specific name derived from the legacy configuration Container
    And a new container with a non-tenant-specific name derived from the legacy configuration Container should have been created with public access of '<ExpectedAccessType>'

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container does not yet exist and only v2 configuration without Container exists in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a tenanted name derived from the logical container name
    And a new container with a tenanted name derived from the logical container name should have been created with public access of '<ExpectedAccessType>'

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container does not yet exist and only v2 configuration with Container and tenant prefix enabled exists in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>' and a Container name with DisableTenantIdPrefix of false
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   | Container                     |
    | testAccountConnectionString | DerivedFromConfiguredTenanted |
    And a new container with a tenanted name derived from the legacy configuration Container should have been created with public access of '<ExpectedAccessType>'

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container does not yet exist and only v2 configuration with Container and tenant prefix disabled exists in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>' and a Container name with DisableTenantIdPrefix of true
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   | Container                       |
    | testAccountConnectionString | DerivedFromConfiguredUntenanted |
    And a new container with a non-tenant-specific name derived from the legacy configuration Container should have been created with public access of '<ExpectedAccessType>'

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container does not yet exist and only v2 configuration without Container exists in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   | Container                  |
    | testAccountConnectionString | DerivedFromLogicalTenanted |
    And a new container with a tenanted name derived from the logical container name should have been created with public access of '<ExpectedAccessType>'

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

# TODO: multiple containers

# --- V2 config, Container does exist ---

Scenario Outline: Container exists and only v2 configuration with Container exists and tenant prefix enabled in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>' and a Container name with DisableTenantIdPrefix of false
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a container with a tenant-specific name derived from the configured Container exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a tenanted name derived from the legacy configuration Container
    And no new container should have been created

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container exists and only v2 configuration with Container exists and tenant prefix disabled in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>' and a Container name with DisableTenantIdPrefix of true
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a container with a non-tenant-specific name derived from the configured Container exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a non-tenant-specific name derived from the legacy configuration Container
    And no new container should have been created

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container exists and only v2 configuration without Container exists in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a container with a tenanted name derived from the logical container name exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a tenanted name derived from the logical container name
    And no new container should have been created

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

# This covers two scenarios:
#   The container was created through normal use of the v2 libraries before we upgraded to v3
#   We manage to create the new container and then crash before storing the relevant new-form
#       configuration in the tenant
Scenario: Container exists and only v2 configuration with Container and tenant prefix enabled exists in tenant properties when configuration migration preparation occurs
    # AccountName is interpreted as a connection string when there's no AccountKeySecretName
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of 'null' and a Container name with DisableTenantIdPrefix of false
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a container with a tenant-specific name derived from the configured Container exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   | Container                     |
    | testAccountConnectionString | DerivedFromConfiguredTenanted |
    And no new container should have been created

Scenario: Container exists and only v2 configuration with Container and tenant prefix disabled exists in tenant properties when configuration migration preparation occurs
    # AccountName is interpreted as a connection string when there's no AccountKeySecretName
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of 'null' and a Container name with DisableTenantIdPrefix of true
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a container with a non-tenant-specific name derived from the configured Container exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   | Container                       |
    | testAccountConnectionString | DerivedFromConfiguredUntenanted |
    And no new container should have been created

Scenario: Container exists and only v2 configuration without Container exists in tenant properties when configuration migration preparation occurs
    # AccountName is interpreted as a connection string when there's no AccountKeySecretName
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of 'null'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a container with a tenanted name derived from the logical container name exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   | Container                  |
    | testAccountConnectionString | DerivedFromLogicalTenanted |
    And no new container should have been created


# --- V2 and V3 config (Container exists, because it always will if V3 config is present) ---

# TODO: Container name in config vs container name in argument
Scenario Outline: Container exists and both v2 and v3 configurations with Container exist in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with a bogus AccountName and an AccessType of 'null' and a Container name with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText and a Container name
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with the name in the V3 configuration exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with the name in the V3 configuration
    And no new container should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

    # !!! v3 with/without container!?
Scenario: Container exists and both v2 and v3 configurations without Container exist in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with a bogus AccountName and an AccessType of 'null'
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with a tenanted name derived from the logical container name exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a tenanted name derived from the logical container name
    And no new container should have been created

Scenario Outline: Container exists and both v2 configuration without container and v3 configuration with Container exist in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with a bogus AccountName and an AccessType of 'null' and a Container name with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText and a Container name
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with the name in the V3 configuration exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with the name in the V3 configuration
    And no new container should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario: Container exists and both v2 and v3 configurations with Container exist in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with a bogus AccountName and an AccessType of 'null' and a Container name with DisableTenantIdPrefix of <DisableTenantIdPrefix>
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText and a Container name
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with the name in the V3 configuration exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new container should have been created

    Examples:
    | DisableTenantIdPrefix |
    | true                  |
    | false                 |

Scenario: Container exists and both v2 and v3 configurations without Container exist in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with a bogus AccountName and an AccessType of 'null'
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText and a Container name
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with a tenanted name derived from the logical container name exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new container should have been created

Scenario: Container exists and both v2 configuration without container and v3 configuration with Container exist in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with a bogus AccountName and an AccessType of 'null'
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText and a Container name
    And a tenant with the property 'sv2' set to the legacy BlobStorageConfiguration
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with the name in the V3 configuration exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new container should have been created


# --- V3 config only (Container exists, because it always will if V3 config is present) ---

Scenario: Container exists and only v3 configuration with Container exists in tenant properties when container requested
    Given this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText and a Container name
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with the name in the V3 configuration exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with the name in the V3 configuration
    And no new container should have been created

Scenario: Container exists and only v3 configuration without Container exists in tenant properties when container requested
    Given this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with a tenanted name derived from the logical container name exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with configuration keys of 'sv2' and 'sv3'
    Then the BlobContainerClient should have access to the container with a tenanted name derived from the logical container name
    And no new container should have been created

Scenario: Container exists and only v3 configuration with Container exists in tenant properties when configuration migration preparation occurs
    Given this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText and a Container name
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with the name in the V3 configuration exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new container should have been created

Scenario: Container exists and only v3 configuration without Container exists in tenant properties when configuration migration preparation occurs
    Given this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv3' set to the v3 BlobContainerConfiguration
    And a container with a tenanted name derived from the logical container name exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with configuration keys of 'sv2' and 'sv3'
    Then IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async should have returned null
    And no new container should have been created

# TODO: neither v2 nor v3 present (for both GetBlobContainerClientFromTenantAsync and MigrateToV3Async)
# Also: integration tests in which we actually write data out with the V2 code and read it back in with the V3 code?
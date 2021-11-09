@perFeatureContainer

Feature: BlobStorageLegacyMigration
    In order to migrate from v2 to v3 of Corvus.Tenancy when using cloud blob storage for tenanted services
    As a developer using Corvus.Storage.Azure.BlobStorage.Tenancy
    I need to be able to work with storage accounts and tenant configurations created through the v2 libraries when using the v3 libraries

# TODO: Container name in config vs container name in argument
Scenario Outline: Container does not yet exist and only v2 configuration exists in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2` set to the legacy BlobStorageConfiguration
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then a new container with a tenant-specific name derived from 'myc' should have been created with public access of '<ExpectedAccessType>'
    And the BlobContainerClient should have access to the container with a tenant-specific name derived from 'myc'

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

Scenario Outline: Container does not yet exist and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2` set to the legacy BlobStorageConfiguration 
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then a new container with a tenant-specific name derived from 'myc' should have been created with public access of '<ExpectedAccessType>'
    And MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   |
    | testAccountConnectionString |

    Examples:
    | ConfiguredAccessType | ExpectedAccessType |
    | null                 | None               |
    | Off                  | None               |
    | Container            | BlobContainer      |
    | Blob                 | Blob               |

# TODO: multiple containers

Scenario Outline: Container exists and only v2 configuration exists in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName and an AccessType of '<ConfiguredAccessType>'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2` set to the legacy BlobStorageConfiguration
    And a container with a tenant-specific name derived from 'myc' exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then no new container should have been created
    And the BlobContainerClient should have access to the container with a tenant-specific name derived from 'myc'

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
Scenario: Container exists and only v2 configuration exists in tenant properties when configuration migration preparation occurs
    # AccountName is interpreted as a connection string when there's no AccountKeySecretName
    Given a legacy BlobStorageConfiguration with an AccountName of 'UseDevelopmentStorage=true' and an AccessType of 'null'
    And this test is using an Azure BlobServiceClient with a connection string
    And a tenant with the property 'sv2` set to the legacy BlobStorageConfiguration
    And a container with a tenant-specific name derived from 'myc' exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then no new container should have been created
    And MigrateToV3Async should have returned a BlobContainerConfiguration with these settings
    | ConnectionStringPlainText   |
    | testAccountConnectionString |


# TODO: Container name in config vs container name in argument
Scenario: Container exists and both v2 and v3 configurations exist in tenant properties when container requested
    Given a legacy BlobStorageConfiguration with an AccountName of 'ThisIsBogusToVerifyThatItIsNotBeUsed' and an AccessType of 'null'
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv2` set to the legacy BlobStorageConfiguration
    And a tenant with the property 'sv3` set to the v3 BlobContainerConfiguration 
    And a container with a tenant-specific name derived from 'myc' exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then no new container should have been created
    And the BlobContainerClient should have access to the container with a tenant-specific name derived from 'myc'

Scenario: Container exists and both v2 and v3 configurations exist in tenant properties when configuration migration preparation occurs
    Given a legacy BlobStorageConfiguration with an AccountName of 'ThisIsBogusToVerifyThatItIsNotBeUsed' and an AccessType of 'null'
    And this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv2` set to the legacy BlobStorageConfiguration 
    And a tenant with the property 'sv3` set to the v3 BlobContainerConfiguration 
    And a container with a tenant-specific name derived from 'myc' exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then no new container should have been created
    And MigrateToV3Async should have returned null

Scenario: Container exists and only v3 configurations exist in tenant properties when container requested
    Given this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv3` set to the v3 BlobContainerConfiguration 
    And a container with a tenant-specific name derived from 'myc' exists
    When IBlobContainerSourceWithTenantLegacyTransition.GetBlobContainerClientFromTenantAsync is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then no new container should have been created
    And the BlobContainerClient should have access to the container with a tenant-specific name derived from 'myc'

Scenario: Container exists and only v3 configurations exist in tenant properties when configuration migration preparation occurs
    Given this test is using an Azure BlobServiceClient with a connection string
    And a v3 BlobContainerConfiguration with a ConnectionStringPlainText
    And a tenant with the property 'sv3` set to the v3 BlobContainerConfiguration 
    And a container with a tenant-specific name derived from 'myc' exists
    When IBlobContainerSourceWithTenantLegacyTransition.MigrateToV3Async is called with a container name of 'myc' and configuration keys of 'sv2' and 'sv3'
    Then no new container should have been created
    And MigrateToV3Async should have returned null

# TODO: neither v2 nor v3 present (for both GetBlobContainerClientFromTenantAsync and MigrateToV3Async)
# TODO: separate tests for the BlobStorageConfiguration -> BlobContainerConfiguration testing all the variations
# Also: integration tests in which we actually write data out with the V2 code and read it back in with the V3 code?
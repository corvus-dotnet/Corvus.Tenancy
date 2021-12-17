﻿Feature: LegacyConfigurationTransformation
    In order to migrate from v2 to v3 of Corvus.Tenancy when using cloud blob storage for tenanted services
    As a developer of Corvus.Storage.Azure.BlobStorage.Tenancy working on migration support
    I need to be able to convert configuration entries created for v2 libraries into the new format

# The V2 libraries used the non-obvious convention that if you did not specify any key vault
# settings, the AccountName would be interpretted as a connection string.
Scenario: Plain text connection string stored in AccountName
	Given legacy v2 configuration with these properties
    | PropertyName | Value              |
    | AccountName  | MyConnectionString |
	When the legacy v2 configuration is converted to the new format
	Then the resulting BlobContainerConfiguration has these properties
    | PropertyName              | Value              |
    | ConnectionStringPlainText | MyConnectionString |

Scenario: Plain text connection string stored in AccountName with container name
	Given legacy v2 configuration with these properties
    | PropertyName | Value              |
    | AccountName  | MyConnectionString |
    | Container    | MyContainerName    |
	When the legacy v2 configuration is converted to the new format
	Then the resulting BlobContainerConfiguration has these properties
    | PropertyName              | Value              |
    | ConnectionStringPlainText | MyConnectionString |
    | Container                 | MyContainerName    |

# A slightly questionable feature of the V2 libraries that some tests and local dev scenarios
# depend on is that if you provide a completely empty configuration, you end up using the local
# storage emulator.
Scenario: All null configuration results in development storage
	Given legacy v2 configuration with these properties
    | PropertyName | Value              |
	When the legacy v2 configuration is converted to the new format
	Then the resulting BlobContainerConfiguration has these properties
    | PropertyName              | Value                      |
    | ConnectionStringPlainText | UseDevelopmentStorage=true |

# A related slightly peculiar feature of the V2 libraries is that it detects the use of the
# standard development storage connection string, and instead of just passing that through
# it instead uses the standard developments storage account name. It didn't set the account
# key but apparently it worked anyway. We're deliberately changing the behaviour and specifying
# use of the storage emulator through the same connection string, instead of turning it into
# the storage account name.
Scenario: Development storage connection results in development storage
	Given legacy v2 configuration with these properties
    | PropertyName | Value                      |
    | AccountName  | UseDevelopmentStorage=true |
	When the legacy v2 configuration is converted to the new format
	Then the resulting BlobContainerConfiguration has these properties
    | PropertyName              | Value                      |
    | ConnectionStringPlainText | UseDevelopmentStorage=true |

Scenario: Account name with secret in key vault
	Given legacy v2 configuration with these properties
    | PropertyName         | Value        |
    | AccountName          | MyAccount    |
    | KeyVaultName         | MyVault      |
    | AccountKeySecretName | MySecretName |
	When the legacy v2 configuration is converted to the new format
	Then the resulting BlobContainerConfiguration has these properties
    | PropertyName        | Value     |
    | AccountName         | MyAccount |
    | AccessKeyInKeyVault | <notnull> |
    And the resulting BlobContainerConfiguration.AccessKeyInKeyVault has these properties
    | PropertyName | Value        |
    | VaultName    | MyVault      |
    | SecretName   | MySecretName |

Scenario: Account name with secret in key vault with container name
	Given legacy v2 configuration with these properties
    | PropertyName         | Value           |
    | AccountName          | MyAccount       |
    | Container            | MyContainerName |
    | KeyVaultName         | MyVault         |
    | AccountKeySecretName | MySecretName    |
	When the legacy v2 configuration is converted to the new format
	Then the resulting BlobContainerConfiguration has these properties
    | PropertyName        | Value           |
    | AccountName         | MyAccount       |
    | AccessKeyInKeyVault | <notnull>       |
    | Container           | MyContainerName |
    And the resulting BlobContainerConfiguration.AccessKeyInKeyVault has these properties
    | PropertyName | Value        |
    | VaultName    | MyVault      |
    | SecretName   | MySecretName |

Feature: LegacyConfigurationTransformation
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

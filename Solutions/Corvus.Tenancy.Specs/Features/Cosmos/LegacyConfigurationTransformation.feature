Feature: LegacyConfigurationTransformation
    In order to migrate from v2 to v3 of Corvus.Tenancy when using cloud blob storage for tenanted services
    As a developer of Corvus.Storage.Azure.Cosmos.Tenancy working on migration support
    I need to be able to convert configuration entries created for v2 libraries into the new format

Scenario Outline: Plain text connection string stored in AccountUri
	Given legacy v2 cosmos storage configuration with these properties
    | PropertyName  | Value                                                                       |
    | AccountUri    | https://mycosmosaccount.documents.azure.com:443/;AccountKey=SuperSecretKey; |
    | DatabaseName  | <DatabaseName>                                                              |
    | ContainerName | <ContainerName>                                                             |
	When the legacy v2 cosmos storage configuration is converted to the new format
	Then the resulting CosmosContainerConfiguration has these properties
    | PropertyName              | Value                                                                       |
    | ConnectionStringPlainText | https://mycosmosaccount.documents.azure.com:443/;AccountKey=SuperSecretKey; |
    | Database                  | <DatabaseName>                                                              |
    | Container                 | <ContainerName>                                                             |

    Examples:
    | DatabaseName | ContainerName |
    |              |               |
    | MyDb         |               |
    |              | MyContainer   |
    | MyDb         | MyContainer   |

Scenario: Account URI with access key in key vault
	Given legacy v2 cosmos storage configuration with these properties
    | PropertyName         | Value                                            |
    | AccountUri           | https://mycosmosaccount.documents.azure.com:443/ |
    | KeyVaultName         | MyVault                                          |
    | AccountKeySecretName | MySecretName                                     |
	When the legacy v2 cosmos storage configuration is converted to the new format
	Then the resulting CosmosContainerConfiguration has these properties
    | PropertyName        | Value                                            |
    | AccountUri          | https://mycosmosaccount.documents.azure.com:443/ |
    | AccessKeyInKeyVault | <notnull>                                        |
    And the resulting CosmosContainerConfiguration.AccessKeyInKeyVault has these properties
    | PropertyName | Value        |
    | VaultName    | MyVault      |
    | SecretName   | MySecretName |

# A slightly questionable feature of the V2 libraries that some tests and local dev scenarios
# depend on is that if you provide a completely empty configuration, you end up using the local
# storage emulator.
Scenario: All null configuration results in development storage
	Given legacy v2 cosmos storage configuration with these properties
    | PropertyName | Value              |
	When the legacy v2 cosmos storage configuration is converted to the new format
	Then the resulting CosmosContainerConfiguration has these properties
    | PropertyName              | Value                                 |
    | ConnectionStringPlainText | $WellKnownCosmosDevelopmentStorageUri |

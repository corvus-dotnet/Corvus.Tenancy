Feature: BlobStorageLegacyConfigKeyNaming
    As a developer needing V2 legacy support when using tenanted Blob Storage
    In order to use V2 configuration from a V3+ application
    I need to be able to discover the tenant property keys that the V2 libraries would have chosen

Scenario: Get tenant property key for V2 blob storage container
	When I get the tenant property key for a logical blob container name of 'foo'
	Then the property key name is 'StorageConfiguration__foo'

Scenario: Get tenant property key for V2 table storage container
	When I get the tenant property key for a logical table storage container name of 'foo'
	Then the property key name is 'StorageConfiguration__Table__foo'

Scenario: Get tenant property key for V2 Cosmos container
	When I get the tenant property key for a logical Cosmos database name of 'foo' and logical container name of 'bar'
	Then the property key name is 'StorageConfiguration__foo__bar'

Scenario: Get tenant property key for V2 SQL container
	When I get the tenant property key for a logical SQL database name of 'foo'
	Then the property key name is 'StorageConfiguration__foo'

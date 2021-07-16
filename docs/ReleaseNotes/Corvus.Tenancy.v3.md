# Release notes for Corvus.Tenancy v3.

## v3.0

Breaking changes:

* Databases, containers, etc. are no longer created on demand, so these must be created by applications as part of their new tenant initialization
* The old storage-specific definition types (`BlobStorageContainerDefinition`, `CosmosContainerDefinition`, etc.) have been removed, and instead we now talk about "storage contexts" (where a "context" is a container, or a SQL db) and these contexts are invariable identified by a simple `string` which we refer to as a `contextName`
* The names of tenant properties in which storage configuration settings go have changed: they used to be ambiguous (with several different providers using `StorageConfiguration__ContainerName`, leading to potential collisions when using multiple storage types), but now they are all prefixed with the name of the configuration type, e.g. `BlobStorageConfiguration__ContainerName`

* replaced deprecated `Microsoft.Azure.Storage.Blob` with `Azure.Storage.Blobs`
* replaced deprecated `Microsoft.Azure.KeyVault` with `Azure.Security.KeyVault.Secrets`
* Various types and methods have been renamed to reflect the fact that the `CloudBlobContainer` in the old Azure client libraries has been replaced with the `BlobContainerClient` type:
  * `AddTenantCloudBlobContainerFactory -> `AddTenantBlobContainerClientFactory` (`IServiceCollection` extension method)
  * `ITenantCloudBlobContainerFactory` -> `ITenantBlobContainerClientFactory`

Note that the NuGet changes mean that there are breaking changes for configuration. `BlobStorageContainerDefinition` and `BlobStorageConfiguration` both support specifying the public access settings for any containers created, and they used to do so using the `BlobContainerPublicAccessType` enumeration type defined by the `Microsoft.Azure.Storage.Blob` package. In the new Azure SDKs, this has been replaced by a type that uses different names for its enumeration entries, meaning that any applications with configuration settings will need to change.

This affects the `AccessType` for any `BlobStorageConfiguration`, and also the `AccessType` for any `BlobStorageContainerDefinition`. (Each of these would normally be stored in the properties of an application tenant.) In practice, it's very unusual to set these properties because the default (no public access) is normally what's needed, but if an application has set these, the following changes will be required for any such settings:

| Old Value | New Value     |
| --------- | ------------  |
| Off       | None          |
| Container | BlobContainer |
| Blob      | Blob          |

(Note: if you had Blob before, no change is required.)


### Removal of automatic creation of dbs, containers, etc

In V2, the first time you tried to use a container for some storage types, the relevant container (and possible containing database) would be created on demand if necessary for certain storage types. (Non-Gremlin-based Cosmos DB, and Azure Storage Tables and Blobs). This was problematic for various reasons. It meant that tenant initialization failures could happen a long time after a tenant was nominally created. It required storage clients to have credentials capable of performing the relevant initialization. It could also lead to conflicts if settings such as partition keys or throughput for the container were incompatible with settings already in place.

So it now becomes the responsibility of the application creating the tenant always to create all necessary databases and containers up front, and ensuring that the relevant configuration in the tenant specifies all the information required to locate these things.

New helper methods are provided by the `ContainerNameHelpers` class in the `Corvus.Azure.Storage.Tenancy` library to build suitable container names out of a scope (e.g., tenant) ID and context name. This will produce the same names previously produced for automatically-created containers.


### Tenant property names for storage configuration

List the various changes.

`BlobStorageConfiguration`
`TableStorageConfiguration`


TBD: the various ITenant*Factory interfaces have all changed
TBD: removed PartitionKeyPath from CosmosConfiguration

Rename TenantXxxConfig types to just XxxConfig
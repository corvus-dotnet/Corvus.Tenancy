# Release notes for Corvus.Tenancy v3.

## v3.0

Breaking changes:

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

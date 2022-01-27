# Release notes for Corvus.Tenancy v3.

## v3.0

Targets .NET 6.0 only.
Initial support for Azure Storage Blobs, and Cosmos DB (SQL) only. Other providers currently slated for v3.1.
Uses new-style (Azure.Core) Azure Client SDK—no more dependencies on deprecated client libraries.
Use `Corvus.Identity` v3.0 to enable use of Azure AD client identities (not possible with `Corvus.Tenancy` v2) or to retrieve access keys or connection strings from Key Vault (v2 had limited support for this)
Automatic container creation no longer occurs—see ADR0003 (but v2-v3 transition support is available—see ADR0004)
Support for key rotation scenarios (applications can report that a container no longer works, and that all cached credential information should be discarded, refetching all details from key vault or other sources)


### Breaking changes

This is very different from v2, and upgrading will require rework, because every one of the new features described above is a breaking change.
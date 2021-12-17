# `Corvus.Tenancy` will not create storage containers automatically

## Status

Accepted

## Context

In `Corvus.Tenancy` v2, the various storage-technology-specific libraries (e.g. `Corvus.Azure.Storage.Tenancy`) could dynamically create new containers for you the first time you asked for them. One of the problems this caused was that the definition types (e.g. `BlobStorageContainerDefinition`) needed to include all of the information required to be able to create a new container on demand. For example, with blob containers, that meant specifying the container's public access type. This was not a great idea, because it muddied the role of the definition types. These were primarily logical names, but they also ended up containing the default configuration settings to use in these auto-container-generation scenarios.

The tenant onboarding process (the process of enabling a new tenant to use an application, or some particular piece of application functionality) necessarily includes these steps:

 * determining the storage account (and relevant credentials) to use
 * picking a suitable container name, ensuring proper tenant isolation
 * creating the container

In V2, applications were in control of the first of these. But in most cases, the second and third were handled by `Corvus.Tenancy`. These last two were unhelpfully tied together because of an unfortunate comingling of concerns. This happened due to good but misguided intentions. We were aiming to enable applications to have a single configuration serving multiple logical containers. For certain kinds of storage (e.g., Azure blob storage) it's common for an application to split data across multiple containers (e.g., putting all the user profile details in one container, and, e.g., to-do list entries in another container). In a non-tenanted application you'd expect to configure settings such as account name and credentials just once—it wouldn't normally make sense to have per-container configuration settings because you'd expect to use the same account across all the logical containers. When it came to tenanted storage, the v2 libraries tried to support the same approach by offering convention-based mechanisms to enable multiple container 'definitions' (logical names) to refer to the same underlying configuration. However, this was inextricably linked to letting the storage libraries pick the container name.

The problem arose because one of the things V2 tried to do for us was to map from the logical container names in the definition types (e.g. `BlobStorageContainerDefinition.ContainerName`) to an actual container name. To enable isolation of data across tenants even when they shared a storage account, this container name mapping would typically incorporate the tenant ID in the real container name. However, this naming scheme was initially an undocumented implementation detail, preventing applications from anticipating what the container would actually be called. If the application doesn't know the container name, it can't create the container itself prior to first use, and so these tenanted storage providers also automatically created the container too.

This is how we ended up with the definition types (e.g. `BlobStorageContainerDefinition`), which were meant to be logical identifiers, needing to include all of the information required to be able to create a new container on demand.

It was technically possible for application code to take control of all three of the steps listed above itself in V2, but it was problematic. You could disable the tenanted container name generation, giving you control over the container name, making it possible for the application to know the right container name, but an unfortunate side effect of this was that for each new tenant, you ended up needing to create one configuration for every logical container. (I.e., the decision to take control of container creation unavoidably meant using more complex configuration.) We did make some changes enabling applications to predict the names that would be using, so they could get in ahead, but with hindsight, we ended up regretting ever making the tenanted storage libraries create containers in the first place.

Another problem with the automatic create-on-first-use behaviour was that any problems that would prevent creation became visible rather late in the day: you might think you'd successfully onboarded a new tenant, only to discover later that not everything is going to work.


## Decision

In `Corvus.Tenancy` v3, applications are responsible for creating any containers they need. The new tenanted storage client libraries will never create a container for you.

Also, applications determine the strategy for picking tenant-qualified names to ensure isolation in cases where multiple tenants are sharing a storage account. The tenancy libraries will provide mechanisms that do most of the work, so the main change is that the application has to opt in explicitly.


## Consequences

Applications will always need to create all of the storage containers they need as part of their tenant onboarding process. This has the following benefits:

* logical container names are separate from storage account settings
* once onboarding is complete, an application can be confident that it has created everything needed for the new tenant, and that there won't be surprise errors later when we first try to use the storage for that tenant

It has these downsides:

* applications will need more code (to pick name, and create containers)


Migration! There's a problem for v2-v3 migration: it's possible for a v2 app to have created a tenant and defined storage configuration, but not yet have attempted to use that configuration for all the logical containers the app needs. When using v2 libraries, these things just get created on demand. But once we switch to a v3 library, it will just fail if it encounters one of these things. So we need a transitional mode. The steps upgrade look like this:

1. running fully v2
2. using v3 libraries, but making some sort of `EnsureContainerExistsToSupportTransitionFromV2` call every time we get a container
    a. without making any modifications to tenant configuration
    b. adding a new V3-form tenant configuration property
3. run a tool that walks the entire tenancy tree ensuring all containers exist
4. disable/remove the transition support
5. remove all the old v2 configuration entries
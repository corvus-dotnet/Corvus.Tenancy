# Tenants will not inherit their parents properties

## Status

Accepted

## Context

Corvus.Tenancy supports a hierarchy of tenants. There are two things that this can be used for:

Firstly, we can control on a per-tenant basis where the child tenant data for that tenant are stored. For example, with two sibling tenants A and B (i.e. tenants that are children of the same parent tenant), the data for the child tenants of A can be stored in a completely different location to that of B. By default, this will be a separate container in the same storage account, but it could be a completely separate storage account.

Secondly, it can be used to enable better organisation of tenants by using parent tenants to group related tenants together.

One of of the functions of tenants is to hold client-specific configuration for the applications that a client is using. An example would be for a client using the Workflow service. Their tenant will contain two pieces of storage information, one for Workflows and one for Workflow instances. This configuration is stored in a collection of key-value pairs attached to the tenant.

It is possible for tenants to have child tenants in the hierarchy. If a tenant that uses the Workflow service has children, they may also need to use the Workflow service. In this case we have a choice: we can decide that we will allow the workflow storage configuration from a tenant to be inherited by its children, or we can require each tenant to contain all of its own configuration.

## Decision

We have determined that we will not make properties of tenants available to their children by default. Applications which consume this library can implement that functionality for themselves if required - for example, by manually copying properties from parent to children when new tenants are created.

Whilst property inheritance seems desirable from a development perspective - for example, creating temporary tenants for testing purposes, or setting up tenants for developers - it is likely to be less useful in envisaged production scenarios.

In the case when hierarchy is used for organisational purposes, inheritance is not relevant; parent tenants are there solely to group their children and configuration for the parent tenant is irrelevant, as it does not exist to be used as a tenant in its own right.

In the case where hierarchy represents a genuine parent-child relationship there are many potential reasons for this, and the goal of the project is not to dictate specific use cases. However, in making the decision not to implement property inheritance it is only necessary to find a use case where it is not desirable.

Our use case here is a PaaS product providing multiple services - endjin's Marain platform. This platform contains several base services - Tenancy, Workflow, Operations and Claims - which can be licensed by clients.

A client may choose to use these services to build their own platform, and use Marain's tenancy service to provide their own platform services to their own customers. In this case, the client's customers will be represented by child tenants of its own tenant. 

In this situation there are two negative outcomes from allowing configuration to inherit from parent to child tenants.
1. The client may make use of Marain services (e.g. Workflow) to provide services to its customers. Configuration for these services is stored as configuration on the client tenant. Automatic property inheritance would mean that by default, child tenants of the client would also have the ability to access these services, which should not be the case. 
1. The configuration attached to a client's tenant contains various pieces of sensitive information. For example, it may contain storage account details for storage that is not directly owned by the client. For this reason, Marain does not allow clients to view their own configuration data, or that of their parents. However, clients do need to be able to view and modify the configuration of child tenants. If we automatically allowed properties to be inherited by child tenants, it would be possible for a client to create a child tenant and examine those inherited properties to access what is effectively the client's own configuration data.

## Consequences

Whilst not providing inheritance as standard reduces the risk of leaking configuration between parent and child tenants, it is possible to envisage scenarios where inheritance would be desirable. In these scenarios it will instead be necessary to manually copy configuration from parents to children, and potentially to maintain multiple copies of it.

In these situations, client applications can simulate inheritance by automating the duplication of properties and by maintaining additional properties that indicate when values are effectively "inherited" vs when they have been explicitly set on the child tenants.
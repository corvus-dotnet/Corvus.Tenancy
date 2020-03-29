# Tenants will not inherit their parents properties

## Status

Proposed

## Context

Corvus.Tenancy supports a hierarchy of tenants. There are two things that this can be used for:

Firstly, we can control on a per-tenant basis where the children of that tenant are stored. For example, with two sibling tenants A and B (i.e. tenants that are children of the same parent tenant), the children of A can be stored in a completely different location to the children of B. By default, this will be a separate container in the same storage account, but it could be a completely separate storage account.

Secondly, it can be used to enable better organisation of tenants by using parent tenants to group related tenants together.

One of of the functions of tenants is to hold client-specific configuration for the applications that a client is using. An example would be for a client using the Workflow service, their tenant will contain two pieces of storage information, one for Workflows and one for Workflow instances. This configuration is stored in a collection of key-value pairs attached to the tenant.

It is possible for tenants to have child tenants in the hierarchy. If a tenant that uses the Workflow service has children, they may also need to use the Workflow service. In this case we have a choice: we can decide that we will allow the workflow storage configuration from a tenant to be inherited by its children, or we can require each tenant to contain all of it's own configuration.

## Decision

We have determined that we will not allow properties of tenants to be inherited by their children.

Whilst property inheritance seems desirable from a development perspective - for example, creating temporary tenants for testing purposes, or setting up tenants for developers - it is likely to be less useful in envisaged production scenarios.

In the case when hierarchy is used for organisational purposes, inheritance is not relevant; parent tenants are there solely to group their children and configuration for the parent tenant is irrelevant, as it does not exist to be used as a tenant in it's own right.

In the case where hierarchy represents a genuine parent-child relationship there are many potential reasons for this, and the goal of the project is not to dictate specific use cases. However, in making the decision not to implement property inheritance it is only necessary to find a use case where it is not desirable.

Our use case here is a PaaS product providing multiple services - endjin's Marain platform. This platform contains several base services - Tenancy, Workflow, Operations and Claims, which can be licenced by clients.

A client may choose to use these services to build their own platform, and use Marain's tenancy service to provide their own platform services to their own customers. In this case, the client's customers will be represented by child tenants of it's own tenant.

Whilst the client tenant may make use of Marain services such as workflow to provide services to its customers, it would not want to leak private information, such as configuration for those Marain services, to customers.

## Consequences

Whilst not providing inheritance as standard reduces the risk of leaking configuration between parent and child tenants, it is possible to envisage scenarios where inheritance would be desirable. In these scenarios it will instead be necessary to manually copy configuration from parents to children, and potentially to maintain multiple copies of it.

In this situations, client applications can simulate inheritance by automating the duplication of properties and by maintaining additional properties that indicate when values are effectively "inherited" vs when they have been explicitly set on the child tenants.
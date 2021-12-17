# `Corvus.Tenancy` provides wrappers to facilitate transition from v2 to v3

## Status

Accepted

## Context

As described in [ADR 0004, `Corvus.Tenancy` will not create storage containers automatically](./0003-no-automatic-storage-container-creation.md), `Corvus.Tenancy` v3 introduces a change: applications are now responsible for creating all necessary containers when onboarding a client. This creates a challenge for applications that have already been deployed on v2, because the following things may be true:

* a tenant may exist in which only a subset of its storage containers exist
* in a no-downtime migration, a compute farm may have a mixture of v2 and v3 components in use

To enable applications currently using `Corvus.Tenancy` v2 to migrate to v3 without disruption, we need a clearly defined path of how a system will be upgraded.

## Decision

Upgrades from v2 to v3 use a multi-phase approach, in which any single compute node in the application goes through these steps:

1. using nothing but v2
1. using v3 libraries mostly (see below) in v2 mode
1. using v3 libraries, onboarding new clients in v3 style, using v3 config where available, falling back to v2 config and auto-creation of containers when v3 config not available
1. using v3 libraries in non-transitional mode

While in phase 3, we would run a tool to transition all v2 configuration to v3. Once this tool has completed its work, we are then free to move into phase 4. (There's no particular hurry to move into this final phase. Once all tenants that had v2 configuration have been migrated to v3, there's no behavioural difference between phases 3 and 4. The main motivation for moving to phase 4 is that it enables applications to remove transitional code once transition is complete. Phase 4 might not occur until years after the other phases. For example, libraries such as [Marain](https://github.com/marain-dotnet) that enable developers to host their own instances of a service might choose to retain transitional code for a very long time to give customers of these libraries time to complete their migration.)

To support zero-downtime upgrades, it's necessary to support a state where all compute nodes using a particular store are in a mixture of two adjacent phases. E.g., when we move from 1 to 2, there will be a period of time in which some nodes are still in phase 1, and some are in phase 2. However, we will avoid ever being in three phases simultaneously. For example, we will wait until all compute nodes have completed their move to state 2 before moving any into state 3.

The following sections describe the behaviour required in each of the v3 states to support transition. (There's nothing to document here for phase 1, because that's how systems already using v2 today behave.)

### Phase 2: using v3 libraries, operating in v2 mode

A node in this phase has upgraded to v3 libraries, but is using the transition support and is essentially operating in v2 mode. It will never create new v3 configuration. New tenants continue to be onboarded in the same way as with v2 libraries—the application does not pre-create containers, and expects the tenancy library to create them on demand as required. This gives applications a low-impact way in which to upgrade to v3 libraries without changing any behaviour, and also opens the path to migration towards the new style of operation.

The one difference in behaviour (the reason we describe this as "mostly" v2 mode above) is that if v3 configuration is present for a particular configuration key, it has the following effects:

 * the application will use the v3 configuration and will not even look to see if v2 configuration is present
 * the application will presume that all relevant containers for this configuration have already been created, and will not attempt to create anything on demand
 
This is necessary to support the case where all nodes have completed their transition to phase 2 (so none is in phase 1), and some have have moved to phase 3. Nodes that are still in phase 2 at this point need to be able to cope with the possibility that some clients have been onboarded by a phase 3 node, and so there will be only v3 configuration available. (We do not expect both v2 and v3 configuration to be present for any particular container at this point, because migration of tenants onboarded the v2 way into v3 configuration does not start until all nodes have reached phase 3.)

To configure a node to run in this mode, use storage through a suitable transitional interface (e.g., `IBlobContainerSourceWithTenantLegacyTransition`). The application must provide two configuration keys: one for v2 configuration and one for v3 configuration. The transitional adapter will never create v3 configuration, but it will look for it, and only looks for v2 configuration when no v3 configuration is present.

### Phase 3: v3 libraries, operating in v3 mode, falling back to v2 as necessary

A node in this phase is using the v3 libraries. When onboarding new tenants, it pre-creates all necessary containers, and stores v3 config, but it still uses the transition support so that in cases where existing tenants have only v2 configuration available, it can fall back to the old behaviour.

The only difference between phase 2 and phase 3 is how the application onboards new tenants. Both phases use the transitional adapter in exactly the same way.

### Configuration migration

Once all nodes are in phase 3, a tool can be run to upgrade all v2 configuration to v3. Some aspects of this tooling are necessarily application-specific: only the application can know how to discover all of its tenants, and only the application can know what configuration it is storing, and under which keys.

# Corvus.Tenancy
[![Build Status](https://dev.azure.com/endjin-labs/Corvus.Tenancy/_apis/build/status/corvus-dotnet.Corvus.Tenancy?branchName=main)](https://dev.azure.com/endjin-labs/Corvus.Tenancy/_build/latest?definitionId=4&branchName=main)
[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/corvus-dotnet/Corvus.Tenancy/main/LICENSE)
[![IMM](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/total?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/total?cache=false)

This project provides storage isolation for multi-tenanted applications and services. It also defines a set of interfaces defining a simple model for multi-tenancy, and a means of retrieving tenant information.

These libraries are built for netstandard2.0.

## Purpose

These libraries provide are designed for use in multi-tenanted services and applications. They provide two main features:

- isolation of each tenant's storage
- an abstraction for storing per-tenant configuration

The fundamental requirement behind these features is when services use shared infrastructure for reasons of operational efficiency, there must be strict separation of each tenant's data. Data owned by one tenant must not become visible to, or be modifiable by another tenant.

A secondary requirement is that some organizations might wish to impose additional boundaries for defense in depth. For example, when storing data in Cosmos DB, a client might demand that a separate Cosmos DB instance be used for each tenant. This incurs additional expense, through both direct Azure billing costs, and the ongoing maintenance overheads of having extra Azure resources to configure and monitor, but for some businesses, this may be a reasonable price to pay for the strict separation of data it offers. Or it might be that they wish to group tenants in such a way that each group gets its own instance, but each tenant within that group gets its own collection. (This might make sense if you are building a multi-tenanted SaaS offering where your customer build their own multi-tenanted systems on top.) And in some cases, there might be no need for any such separation, in which case it may be appropriate to use a single Cosmos DB instance with one collection per tenant. We support all of these variations.

Isolated storage is provided by various storage-technology-specific libraries described in the next section.

## Features

This project provides several libraries, which break down into three areas: abstractions defining the model by which tenants are represented, various storage-technology-specific tenanted storage providers, and an implementation of a store that keeps track of which tenants exist, and holds their configuration.

### Tenant model
The [`Corvus.Tenancy.Abstractions`](https://www.nuget.org/packages/Corvus.Tenancy.Abstractions/) library provides basic Tenant features:

- The `ITenant` interface, for working with tenants—configuration and properties are accessed via `ITenant`
- The `ITenantProvider` interface, an abstraction for storage and retrieval of tenants, and navigation of the hierarchy of tenants-of-tenants

### Tenanted storage providers
Each supported storage technology has a corresponding library:

- [`Corvus.Azure.Cosmos.Tenancy`](https://www.nuget.org/packages/Corvus.Azure.Cosmos.Tenancy/): Cosmos SDK V3 Containers
- [`Corvus.Azure.Gremlin.Tenancy`](https://www.nuget.org/packages/Corvus.Azure.Gremlin.Tenancy/): Cosmos Container accessed through the Gremlin API
- [`Corvus.Azure.Storage.Tenancy`](https://www.nuget.org/packages/Corvus.Azure.Storage.Tenancy/): Azure Storage Blob Containers
- [`Corvus.Sql.Tenancy`](https://www.nuget.org/packages/Corvus.Sql.Tenancy/): Azure SQL and SQL Server databases

### Tenant store implementation

The [`Corvus.Tenancy.Storage.Azure.Blob`](https://www.nuget.org/packages/Corvus.Tenancy.Storage.Azure.Blob/) library provides an implementation of the `ITenantProvider` abstraction on top of Azure Blob Storage. Whereas `Corvus.Azure.Storage.Tenancy` provides tenanted storage, and depends upon some `ITenantProvider` to discover the configuration it requires, `Corvus.Tenancy.Storage.Azure.Blob` is not intended for use as part of the main implementation of multi-tenanted services; it provides a single (non-tenanted) store of the configuration that the various tenanted storage providers require.

In short, this is tenant storage, not tenanted storage. This stores the tenant details. Conversely, the providers listed above use the tenant details to provide a tenanted storage service. The tenanted storage providers are clients of the tenant store.

The intended usage model is that the tenant storage should be a distinct service. Our https://github.com/marain-dotnet/Marain.Tenancy service uses `Corvus.Tenancy.Storage.Azure.Blob` internally, and presents a web API to make the tenant details available. It also provides an implementation of `ITenantProvider` that sits on top of a client for that web API. This is how endjin uses tenancy—that's service exclusively owns the underlying storage holding tenant details, and all other services talk to `Marain.Tenancy` to obtain the details they require.

## Licenses

[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/corvus-dotnet/Corvus.Tenancy/main/LICENSE)

Corvus.Tenancy is available under the Apache 2.0 open source license.

For any licensing questions, please email [&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;](&#109;&#97;&#105;&#108;&#116;&#111;&#58;&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;)

## Project Sponsor

This project is sponsored by [endjin](https://endjin.com), a UK based Microsoft Gold Partner for Cloud Platform, Data Platform, Data Analytics, DevOps, and a Power BI Partner.

For more information about our products and services, or for commercial support of this project, please [contact us](https://endjin.com/contact-us). 

We produce two free weekly newsletters; [Azure Weekly](https://azureweekly.info) for all things about the Microsoft Azure Platform, and [Power BI Weekly](https://powerbiweekly.info).

Keep up with everything that's going on at endjin via our [blog](https://blogs.endjin.com/), follow us on [Twitter](https://twitter.com/endjin), or [LinkedIn](https://www.linkedin.com/company/1671851/).

Our other Open Source projects can be found on [GitHub](https://endjin.com/open-source)

## Code of conduct

This project has adopted a code of conduct adapted from the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community. This code of conduct has been [adopted by many other projects](http://contributor-covenant.org/adopters/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;](&#109;&#097;&#105;&#108;&#116;&#111;:&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;) with any additional questions or comments.

## IP Maturity Matrix (IMM)

The IMM is endjin's IP quality framework.

[![Shared Engineering Standards](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/74e29f9b-6dca-4161-8fdd-b468a1eb185d?nocache=true)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/74e29f9b-6dca-4161-8fdd-b468a1eb185d?cache=false)

[![Coding Standards](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/f6f6490f-9493-4dc3-a674-15584fa951d8?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/f6f6490f-9493-4dc3-a674-15584fa951d8?cache=false)

[![Executable Specifications](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/bb49fb94-6ab5-40c3-a6da-dfd2e9bc4b00?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/bb49fb94-6ab5-40c3-a6da-dfd2e9bc4b00?cache=false)

[![Code Coverage](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/0449cadc-0078-4094-b019-520d75cc6cbb?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/0449cadc-0078-4094-b019-520d75cc6cbb?cache=false)

[![Benchmarks](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/64ed80dc-d354-45a9-9a56-c32437306afa?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/64ed80dc-d354-45a9-9a56-c32437306afa?cache=false)

[![Reference Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/2a7fc206-d578-41b0-85f6-a28b6b0fec5f?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/2a7fc206-d578-41b0-85f6-a28b6b0fec5f?cache=false)

[![Design & Implementation Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/f026d5a2-ce1a-4e04-af15-5a35792b164b?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/f026d5a2-ce1a-4e04-af15-5a35792b164b?cache=false)

[![How-to Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/145f2e3d-bb05-4ced-989b-7fb218fc6705?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/145f2e3d-bb05-4ced-989b-7fb218fc6705?cache=false)

[![Date of Last IP Review](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/da4ed776-0365-4d8a-a297-c4e91a14d646?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/da4ed776-0365-4d8a-a297-c4e91a14d646?cache=false)

[![Framework Version](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/6c0402b3-f0e3-4bd7-83fe-04bb6dca7924?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/6c0402b3-f0e3-4bd7-83fe-04bb6dca7924?cache=false)

[![Associated Work Items](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/79b8ff50-7378-4f29-b07c-bcd80746bfd4?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/79b8ff50-7378-4f29-b07c-bcd80746bfd4?cache=false)

[![Source Code Availability](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/30e1b40b-b27d-4631-b38d-3172426593ca?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/30e1b40b-b27d-4631-b38d-3172426593ca?cache=false)

[![License](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/d96b5bdc-62c7-47b6-bcc4-de31127c08b7?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/d96b5bdc-62c7-47b6-bcc4-de31127c08b7?cache=false)

[![Production Use](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/87ee2c3e-b17a-4939-b969-2c9c034d05d7?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/87ee2c3e-b17a-4939-b969-2c9c034d05d7?cache=false)

[![Insights](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/71a02488-2dc9-4d25-94fa-8c2346169f8b?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/71a02488-2dc9-4d25-94fa-8c2346169f8b?cache=false)

[![Packaging](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/547fd9f5-9caf-449f-82d9-4fba9e7ce13a?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/547fd9f5-9caf-449f-82d9-4fba9e7ce13a?cache=false)

[![Deployment](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/edea4593-d2dd-485b-bc1b-aaaf18f098f9?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/corvus-dotnet/Corvus.Tenancy/rule/edea4593-d2dd-485b-bc1b-aaaf18f098f9?cache=false)

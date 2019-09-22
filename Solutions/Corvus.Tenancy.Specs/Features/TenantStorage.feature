@setupContainer
@withBlobStorageTenantProvider

Feature: TenantStorage
	In order to manage tenants and their configuration
	As a tenant owner
	I want to be able to manage tenants

Scenario: Get a tenant that does not exist
	When I get a tenant with id "NotFound"
	Then it should throw a TenantNotFoundException

Scenario: Get a tenant with an etag retrieved from a created tenant
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the ETag of the tenant called "ChildTenant1" and call it "ChildTenantETag"
	When I get the tenant with the id called "ChildTenantId" and the ETag called "ChildTenantETag"
	Then it should throw a TenantNotModifiedException

Scenario: Get a tenant with an etag retrieved from a tenant got from the repo
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	And I get the ETag of the tenant called "Result" and call it "ResultETag"
	When I get the tenant with the id called "ChildTenantId" and the ETag called "ResultETag"
	Then it should throw a TenantNotModifiedException

Scenario: Create a child tenant
	Given I create a child tenant called "ChildTenant1" for the root tenant
	When I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"

Scenario: Update a child tenant
	Given I create a child tenant called "ChildTenant1" for the root tenant
	When I update the properties of the tenant called "ChildTenant1"
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| SecondKey | This is a string | string         |
	| ThirdKey  | 1999-01-17       | datetimeoffset |
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"
	And the tenant called "Result" should have the properties
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| SecondKey | This is a string | string         |
	| ThirdKey  | 1999-01-17       | datetimeoffset |

Scenario: Create a child of a child
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I create a child tenant called "ChildTenant2" for the tenant called "ChildTenant1"
	When I get the tenant id of the tenant called "ChildTenant2" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant2" should have the same ID as the tenant called "Result"

Scenario: Get children
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I create a child tenant called "ChildTenant2" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant3" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant4" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant5" for the tenant called "ChildTenant1"
	When I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the children of the tenant with the id called "ChildTenantId" with maxItems 20 and call them "Result"
	Then the ids of the children called "Result" should match the ids of the tenants called
	| TenantName   |
	| ChildTenant2 |
	| ChildTenant3 |
	| ChildTenant4 |
	| ChildTenant5 |

Scenario: Get children with continuation token
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I create a child tenant called "ChildTenant2" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant3" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant4" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant5" for the tenant called "ChildTenant1"
	When I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the children of the tenant with the id called "ChildTenantId" with maxItems 2 and call them "Result"
	And I get the children of the tenant with the id called "ChildTenantId" with maxItems 20 and continuation token "Result" and call them "Result2"
	Then there should be 2 tenants in "Result"
	And there should be 2 tenants in "Result2"
	And the ids of the children called "Result" and "Result2" should each match 2 of the ids of the tenants called
	| TenantName   |
	| ChildTenant5 |
	| ChildTenant4 |
	| ChildTenant3 |
	| ChildTenant2 |

Scenario: Delete a child
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I create a child tenant called "ChildTenant2" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant3" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant4" for the tenant called "ChildTenant1"
	And I create a child tenant called "ChildTenant5" for the tenant called "ChildTenant1"
	When I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant id of the tenant called "ChildTenant3" and call it "DeletedChildTenantId"
	And I delete the tenant with the id called "DeletedChildTenantId"
	And I get the children of the tenant with the id called "ChildTenantId" with maxItems 20 and call them "Result"
	Then the ids of the children called "Result" should match the ids of the tenants called
	| TenantName   |
	| ChildTenant2 |
	| ChildTenant4 |
	| ChildTenant5 |
@setupContainer
@withBlobStorageTenantProvider

Feature: TenantStorage
	In order to manage tenants and their configuration
	As a tenant owner
	I want to be able to manage tenants

Scenario: Create a child tenant
	Given I create a child tenant called 'ChildTenant1' for the root tenant
	When I get the tenant id of the tenant called 'ChildTenant1' and call it 'ChildTenantId'
	And I get the tenant with the id called 'ChildTenantId' and call it 'Result'
	Then the tenant called 'Result' should be the same as the tenant called 'ChildTenant1'

Scenario: Update a child tenant
	Given I create a child tenant called 'ChildTenant1' for the root tenant
	When I update the properties of the tenant called 'ChildTenant1'
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| SecondKey | This is a string | string         |
	| ThirdKey  | 1999-01-17       | datetimeoffset |
	And I get the tenant id of the tenant called 'ChildTenant1' and call it 'ChildTenantId'
	And I get the tenant with the id called 'ChildTenantId' and call it 'Result'
	Then the tenant called 'Result' should be the same as the tenant called 'ChildTenant1'
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

Scenario: Create a child of the root tenant with a well known Id
	Given I create a well known child tenant called "ChildTenant1" with a Guid of "F446F305-993B-49A4-B5FA-010EE2AF0FA2" for the root tenant
	Then The tenant called "ChildTenant1" has tenant Id "05f346f43b99a449b5fa010ee2af0fa2"

Scenario: Create a child of a child with well known Ids
	Given I create a well known child tenant called "ChildTenant1" with a Guid of "EE17B20B-B372-4493-8145-9DD95516B9AF" for the root tenant
	And I create a well known child tenant called "ChildTenant2" with a Guid of "DD045C05-E7FB-4214-8878-F9E7CA9B0F5F" for tenant called "ChildTenant1"
	Then The tenant called "ChildTenant1" has tenant Id "0bb217ee72b3934481459dd95516b9af"
	And The tenant called "ChildTenant2" has tenant Id "0bb217ee72b3934481459dd95516b9af055c04ddfbe714428878f9e7ca9b0f5f"

Scenario: Creating a child of a child with a well known Id that is already in use by a child of the same parent throws an ArgumentException
	Given I create a well known child tenant called "ChildTenant1" with a Guid of "ABE7C6C9-8494-4797-B52E-5C7B3EF1CE56" for the root tenant
	And I create a well known child tenant called "ChildTenant2" with a Guid of "DD045C05-E7FB-4214-8878-F9E7CA9B0F5F" for tenant called "ChildTenant1"
	And I create a well known child tenant called "ChildTenant3" with a Guid of "DD045C05-E7FB-4214-8878-F9E7CA9B0F5F" for tenant called "ChildTenant1"
	Then an "ArgumentException" is thrown

Scenario: Creating children that have the same well known Ids under different parents succeeds
	Given I create a well known child tenant called "ChildTenant1" with a Guid of "2A182D6E-FF13-4A73-87AF-0B58D8243603" for the root tenant
	And I create a well known child tenant called "ChildTenant2" with a Guid of "086D75A1-DA07-4C90-BEA7-857A6C126280" for the root tenant
	And I create a well known child tenant called "ChildTenant3" with a Guid of "2A182D6E-FF13-4A73-87AF-0B58D8243603" for tenant called "ChildTenant1"
	And I create a well known child tenant called "ChildTenant4" with a Guid of "2A182D6E-FF13-4A73-87AF-0B58D8243603" for tenant called "ChildTenant2"
	Then no exception is thrown
	And The tenant called "ChildTenant3" has tenant Id "6e2d182a13ff734a87af0b58d82436036e2d182a13ff734a87af0b58d8243603"
	And The tenant called "ChildTenant4" has tenant Id "a1756d0807da904cbea7857a6c1262806e2d182a13ff734a87af0b58d8243603"

Scenario: Add properties to a child tenant that has no properties
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

Scenario: Modify properties of a child tenant
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I update the properties of the tenant called "ChildTenant1"
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| SecondKey | This is a string | string         |
	| ThirdKey  | 1999-01-17       | datetimeoffset |
	When I update the properties of the tenant called "ChildTenant1" and call the returned tenant "UpdateResult"
	| Key       | Value                      | Type    |
	| FirstKey  | 2                          | integer |
	| SecondKey | This is a different string | string  |
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"
	And the tenant called "UpdateResult" should have the properties
	| Key       | Value                      | Type           |
	| FirstKey  | 2                          | integer        |
	| SecondKey | This is a different string | string         |
	| ThirdKey  | 1999-01-17                 | datetimeoffset |
	And the tenant called "Result" should have the properties
	| Key       | Value                      | Type           |
	| FirstKey  | 2                          | integer        |
	| SecondKey | This is a different string | string         |
	| ThirdKey  | 1999-01-17                 | datetimeoffset |

Scenario: Add properties to a child tenant that already has properties
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I update the properties of the tenant called "ChildTenant1"
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| SecondKey | This is a string | string         |
	| ThirdKey  | 1999-01-17       | datetimeoffset |
	When I update the properties of the tenant called "ChildTenant1" and call the returned tenant "UpdateResult"
	| Key       | Value                      | Type    |
	| FourthKey | 2                          | integer |
	| FifthKey  | This is a different string | string  |
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"
	And the tenant called "UpdateResult" should have the properties
	| Key       | Value                      | Type           |
	| FirstKey  | 1                          | integer        |
	| SecondKey | This is a string           | string         |
	| ThirdKey  | 1999-01-17                 | datetimeoffset |
	| FourthKey | 2                          | integer        |
	| FifthKey  | This is a different string | string         |
	And the tenant called "Result" should have the properties
	| Key       | Value                      | Type           |
	| FirstKey  | 1                          | integer        |
	| SecondKey | This is a string           | string         |
	| ThirdKey  | 1999-01-17                 | datetimeoffset |
	| FourthKey | 2                          | integer        |
	| FifthKey  | This is a different string | string         |

Scenario: Remove properties from a child tenant
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I update the properties of the tenant called "ChildTenant1"
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| SecondKey | This is a string | string         |
	| ThirdKey  | 1999-01-17       | datetimeoffset |
	When I remove the "SecondKey" property of the tenant called "ChildTenant1" and call the returned tenant "UpdateResult"
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"
	And the tenant called "Result" should have the properties
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| ThirdKey  | 1999-01-17       | datetimeoffset |
	And the tenant called "UpdateResult" should have the properties
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| ThirdKey  | 1999-01-17       | datetimeoffset |

Scenario: Add, modify, and remove properties of a child tenant that already has properties
	Given I create a child tenant called "ChildTenant1" for the root tenant
	And I update the properties of the tenant called "ChildTenant1"
	| Key       | Value            | Type           |
	| FirstKey  | 1                | integer        |
	| SecondKey | This is a string | string         |
	| ThirdKey  | 1999-01-17       | datetimeoffset |
	When I update the properties of the tenant called "ChildTenant1" and remove the "ThirdKey" property and call the returned tenant "UpdateResult"
	| Key       | Value                      | Type    |
	| SecondKey | This is a different string | string  |
	| FourthKey | 2                          | integer |
	| FifthKey  | This is a new string       | string  |
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"
	And the tenant called "UpdateResult" should have the properties
	| Key       | Value                      | Type           |
	| FirstKey  | 1                          | integer        |
	| SecondKey | This is a different string | string         |
	| FourthKey | 2                          | integer        |
	| FifthKey  | This is a new string       | string         |
	And the tenant called "Result" should have the properties
	| Key       | Value                      | Type           |
	| FirstKey  | 1                          | integer        |
	| SecondKey | This is a different string | string         |
	| FourthKey | 2                          | integer        |
	| FifthKey  | This is a new string       | string         |

Scenario: Rename a child tenant
	Given I create a child tenant called "ChildTenant1" for the root tenant
	When I change the name of the tenant called "ChildTenant1" to "NewName" and call the returned tenant "UpdateResult"
	And I get the tenant id of the tenant called "ChildTenant1" and call it "ChildTenantId"
	And I get the tenant with the id called "ChildTenantId" and call it "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"
	Then the tenant called "ChildTenant1" should have the same ID as the tenant called "Result"

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
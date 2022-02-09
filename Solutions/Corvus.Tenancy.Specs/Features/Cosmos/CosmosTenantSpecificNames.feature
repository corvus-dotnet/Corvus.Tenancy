Feature: CosmosTenantSpecificNames
    As a developer using tenanted Blob Storage
    In order to separate tenant data even when multiple tenants are sharing an Azure Storage Account
    I need to be able to generate tenanted names that meet Azure Storage's container constraints, and which are unique

Scenario: Same tenant different logical database names
    Given a tenant labelled 't1'
    When I get a Cosmos database name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a Cosmos database name for tenant 't1' with a logical name of 'c2' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same tenant different logical container names
    Given a tenant labelled 't1'
    When I get a Cosmos container name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a Cosmos container name for tenant 't1' with a logical name of 'c2' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same logical database name different tenants
    Given a tenant labelled 't1'
    And a tenant labelled 't2'
    When I get a Cosmos database name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a Cosmos database name for tenant 't2' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same logical container name different tenants
    Given a tenant labelled 't1'
    And a tenant labelled 't2'
    When I get a Cosmos container name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a Cosmos container name for tenant 't2' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Asking for the same database name twice
    Given a tenant labelled 't1'
    When I get a Cosmos database name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a Cosmos database name for tenant 't1' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are the same

Scenario: Asking for the same container name twice
    Given a tenant labelled 't1'
    When I get a Cosmos container name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a Cosmos container name for tenant 't1' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are the same

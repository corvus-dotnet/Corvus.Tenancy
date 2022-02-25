Feature: TableNaming
    As a developer using tenanted Azure Table Storage
    In order to separate tenant data even when multiple tenants are sharing an Azure Storage account or Cosmos DB account
    I need to be able to generate tenanted names that meet Azure Storage's container constraints, and which are unique

Scenario: Same tenant different logical names
    Given a tenant labelled 't1'
    When I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenant 't1' with a logical name of 'c2' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same logical name different tenants
    Given a tenant labelled 't1'
    And a tenant labelled 't2'
    When I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenant 't2' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Asking for the same name twice
    Given a tenant labelled 't1'
    When I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are the same

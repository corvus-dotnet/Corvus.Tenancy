Feature: TableNaming
    As a developer using tenanted Azure Table Storage
    In order to separate tenant data even when multiple tenants are sharing an Azure Storage account or Cosmos DB account
    I need to be able to generate tenanted names that meet Azure Storage's container constraints, and which are unique

Scenario: Same tenant different logical names via ITenant
    Given a tenant labelled 't1'
    When I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenant 't1' with a logical name of 'c2' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same tenant different logical names via tenantId
    When I get an Azure table name for tenantId 'f26450ab1668784bb327951c8b08f347' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenantId 'f26450ab1668784bb327951c8b08f347' with a logical name of 'c2' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same logical name different tenants via ITenant
    Given a tenant labelled 't1'
    And a tenant labelled 't2'
    When I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenant 't2' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same logical name different tenants via tenantId
    When I get an Azure table name for tenantId 'f26450ab1668784bb327951c8b08f347' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenantId '3633754ac4c9be44b55bfe791b1780f1' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Asking for the same name twice via ITenant
    Given a tenant labelled 't1'
    When I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenant 't1' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are the same

Scenario: Asking for the same name twice via tenantId
    When I get an Azure table name for tenantId 'f26450ab1668784bb327951c8b08f347' with a logical name of 'c1' and label the result 'n1'
    And I get an Azure table name for tenantId 'f26450ab1668784bb327951c8b08f347' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are the same

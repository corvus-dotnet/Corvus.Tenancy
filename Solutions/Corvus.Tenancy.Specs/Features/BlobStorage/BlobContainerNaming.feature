Feature: BlobContainerNaming
    As a developer using tenanted Blob Storage
    In order to separate tenant data even when multiple tenants are sharing an Azure Storage Account
    I need to be able to generate tenanted names that meet Azure Storage's container constraints, and which are unique

Scenario: Same tenant different logical names
    Given a tenant labelled 't1'
    When I get a blob container for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a blob container for tenant 't1' with a logical name of 'c2' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Same logical name different tenants
    Given a tenant labelled 't1'
    And a tenant labelled 't2'
    When I get a blob container for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a blob container for tenant 't2' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are different

Scenario: Asking for the same name twice
    Given a tenant labelled 't1'
    When I get a blob container for tenant 't1' with a logical name of 'c1' and label the result 'n1'
    And I get a blob container for tenant 't1' with a logical name of 'c1' and label the result 'n2'
    Then the returned container names 'n1' and 'n2' are the same

# This tests against known-good behaviour for how Marain.Tenancy stores tenant data - we are checking
# the names of the containers that hold children for the root, 'Service Tenants' and 'Client Tenants'
# tenants.
Scenario Outline: Well known names
    Given a tenant labelled 't1' with id '<tenantId>'
    When I get a blob container for tenant 't1' with a logical name of '<logicalName>' and label the result 'n1'
    Then the name returned container name 'n1' should be '<physicalName>'

    Examples:
    | tenantId                         | logicalName   | physicalName                             |
    | f26450ab1668784bb327951c8b08f347 | corvustenancy | cce7b3deef3998aad88f5f0116f922a94e7cb6c4 |
    | 3633754ac4c9be44b55bfe791b1780f1 | corvustenancy | 513162c27e77e52411ececa40f1e615455b01fc5 |
    | 75b9261673c2714681f14c97bc0439fb | corvustenancy | 8f33344016814e24b39748c7d33e9da6f7772875 |

@perScenarioContainer
@setupTenantedSqlConnection

Feature: SqlConnection
    In order to use SQL Server storage for tenanted services
    As a developer
    I want to be able to manage the SqlConnection

Scenario: Create a sql connection from a plain text connection string
    Given I have added a SqlDatabaseConfiguration with the connection string 'Server=(localdb)\\mssqllocaldb;Trusted_Connection=True' to a tenant as 'MyConnection'
    When I get the tenanted SqlConnection as 'MyConnection'
    Then the connection string should be 'Server=(localdb)\\mssqllocaldb;Trusted_Connection=True'
        
Scenario: Remove configuration from tenant
    Given I have added a SqlDatabaseConfiguration with the connection string 'MyConnectionString' to a tenant as 'MyConnection'
    When I remove the Sql configuration 'MyConnection' from the tenant
    Then attempting to get the Sql configuration 'MyConnection' from the tenant throws an InvalidOperationException
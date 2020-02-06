@setupContainer
@setupTenantedSqlConnection

Feature: SqlConnection
	In order to use SQL Server storage for tenanted services
	As a developer
	I want to be able to manage the SqlConnection


Scenario: Create a sql connection
	Then I should be able to get the tenanted SqlConnection

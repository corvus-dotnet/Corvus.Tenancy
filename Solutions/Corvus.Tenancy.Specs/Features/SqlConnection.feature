@setupContainer
@setupTenantedSqlConnection
Feature: SqlConnection
	In order to use SQL Server storage for tenanted services
	As a developer
	I want to be able to manage the SqlConnection

Scenario: Create a sql connection
	Then I should be able to get the tenanted SqlConnection

Scenario Outline: Validate a SqlConfiguration
	Given a SqlConfiguration
		| ConnectionString   | KeyVaultName   | ConnectionStringSecretName   |
		| <ConnectionString> | <KeyVaultName> | <ConnectionStringSecretName> |
	When I validate the configuration
	Then the result should be <Result>

	Examples:
		| ConnectionString | KeyVaultName | ConnectionStringSecretName | Result  |
		|                  |              |                            | valid   |
		| Something        |              |                            | valid   |
		| Something        | Something    |                            | invalid |
		| Something        |              | Something                  | invalid |
		| Something        | Something    | Something                  | invalid |
		|                  | Something    |                            | invalid |
		|                  |              | Something                  | invalid |
		|                  | Something    | Something                  | valid   |
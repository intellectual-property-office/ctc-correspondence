Feature: ExternalCorrespondenceApiTests


Scenario: Get  Pending Notifications by OrganisationID
	When I call GET /"<OrganisationID>/pending"
	Then the response status should be "<ExpectedStatus>"
    And for successful responses the output media type should be "application/json"
    And for successful responses, the response should contain header Content-Version with value matching the app version

	Examples: 
	| OrganisationID                       | ExpectedStatus				|
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | OK							|
	|                                      | NotFound					|
	| TestString                           | UnprocessableEntity		|
	| 38f968d3-a8b9-49af-90d1              | UnprocessableEntity		|



Scenario Outline: Get Sent Notifications by OrganisationID and Timestamp Range
  When I call GET /"list/<OrganisationID>" with query parameters "from=<FromDate>" and "to=<ToDate>"
  Then the response status should be "<ExpectedStatus>"
  And for successful responses the output media type should be "application/json"
  And for successful responses, the response should contain header Content-Version with value matching the app version

	Examples:
	| OrganisationID                       | FromDate    | ToDate      | ExpectedStatus       |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-07-01  | 2023-07-25  | OK                   |
	|                                      | 2023-07-01  | 2023-07-25  | NotFound             |
	| TestString                           | 2023-07-01  | 2023-07-25  | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1              | 2023-07-01  | 2023-07-25  | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 |             | 2023-07-25  | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-07-01  |             | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-07-01  |             | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 |             |             | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-02-40  | 2023-07-25  | UnprocessableEntity  |



Scenario: Get (Peek at) Notifications by CorrespondenceID
	When I call GET /"peek/<CorrespondenceID>"
	Then the response status should be "<ExpectedStatus>"
    And for successful responses the output media type should be "application/json"
    And for successful responses, the response should contain header Content-Version with value matching the app version

	Examples: 
	| CorrespondenceID                     | ExpectedStatus				|
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | OK							|
	|                                      | NotFound					|
	| TestString                           | UnprocessableEntity		|
	| 38f968d3-a8b9-49af-90d1              | UnprocessableEntity		|
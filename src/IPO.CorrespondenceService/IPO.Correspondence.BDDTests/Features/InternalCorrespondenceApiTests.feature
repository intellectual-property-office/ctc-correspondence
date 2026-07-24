Feature: InternalCorrespondenceApiTests



Scenario: Get Pending Notifications by OrganisationID
	When I call GET /"internal/<OrganisationID>/pending"
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
  When I call GET /"internal/list/<OrganisationID>" with query parameters "from=<FromDate>" and "to=<ToDate>"
  Then the response status should be "<ExpectedStatus>"
  And for successful responses the output media type should be "application/json"
  And for successful responses, the response should contain header Content-Version with value matching the app version

	Examples:
	| OrganisationID                       | FromDate    | ToDate      | ExpectedStatus       |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-07-01  | 2023-07-25  | OK                   |
	|                                      | 2023-07-01  | 2023-07-25  | MethodNotAllowed     |
	| TestString                           | 2023-07-01  | 2023-07-25  | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1              | 2023-07-01  | 2023-07-25  | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 |             | 2023-07-25  | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-07-01  |             | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-07-01  |             | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 |             |             | UnprocessableEntity  |
	| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | 2023-02-40  | 2023-07-25  | UnprocessableEntity  |



Scenario Outline: Sending a Notification
  Given I set the input media type to "<MediaType>"
  When I call POST /"internal/<OrganisationID>" with payload "<Payload>"
  Then the response status should be "<ExpectedStatus>"
  And for successful responses the output media type should be "application/json"
  And for successful responses, the response should contain header Content-Version with value matching the app version

Examples:
| OrganisationID                       | MediaType        | Payload             | ExpectedStatus         |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | application/json | ValidPayload		| Accepted               |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | application/json | MalformedPayload    | UnprocessableEntity    |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | application/json | EmptyPayload		| UnprocessableEntity    |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | text/xml         | XmlPayload          | UnsupportedMediaType   |
| TestString                           | application/json | ValidPayload		| UnprocessableEntity    |
| 38f968d3-a8b9-49af-90d1              | application/json | ValidPayload		| UnprocessableEntity    |
|                                      | application/json | ValidPayload		| NotFound			     |


Scenario Outline: Post email with file Attachment
  Given I set the input media type to "<MediaType>"
  And I set the input content field media type to "<ContentFieldMediaType>"
  And I set the file type to "<FileType>"
  And I set the number of file attachments to <NumberOfFiles>
  When I call POST /"internal/emailWithFile/<OrganisationID>" with payload "<Payload>" and file attachment
  Then the response status should be "<ExpectedStatus>"
  And for successful responses the output media type should be "application/json"
  And for successful responses, the response should contain header Content-Version with value matching the app version

Examples:
| OrganisationID                       | MediaType           | ContentFieldMediaType | FileType                 | NumberOfFiles | Payload          | ExpectedStatus       |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/pdf          | 1             | ValidPayload     | Accepted             |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | text/csv                 | 1             | ValidPayload     | Accepted             |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/octet-stream | 1             | ValidPayload     | Accepted             |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/pdf          | 2             | ValidPayload     | UnprocessableEntity  |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/pdf          | 0             | ValidPayload     | UnprocessableEntity  |
| TestString                           | multipart/form-data | application/json      | application/pdf          | 1             | ValidPayload     | UnprocessableEntity  |  
| 38f968d3-a8b9-49af-90d1              | multipart/form-data | application/json      | application/pdf          | 1             | ValidPayload     | UnprocessableEntity  |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | application/json    |                       | application/pdf          | 0             | ValidPayload     | UnsupportedMediaType | 
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | text/xml            |                       | application/pdf          | 0             | XmlPayload       | UnsupportedMediaType |



Scenario Outline: Post Precompiled letter request with file Attachment
  Given I set the input media type to "<MediaType>"
  And I set the input content field media type to "<ContentFieldMediaType>"
  And I set the file type to "<FileType>"
  And I set the number of file attachments to <NumberOfFiles>
  When I call POST /"internal/precompiledLetter/<OrganisationID>" with payload "<Payload>" and file attachment
  Then the response status should be "<ExpectedStatus>"
  And for successful responses the output media type should be "application/json"
  And for successful responses, the response should contain header Content-Version with value matching the app version

Examples:
| OrganisationID                       | MediaType           | ContentFieldMediaType | FileType                 | NumberOfFiles | Payload          | ExpectedStatus       |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/pdf          | 1             | ValidPayload     | Accepted             |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | text/csv                 | 1             | ValidPayload     | Accepted             |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/octet-stream | 1             | ValidPayload     | Accepted             |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/pdf          | 2             | ValidPayload     | UnprocessableEntity  |  
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | multipart/form-data | application/json      | application/pdf          | 0             | ValidPayload     | UnprocessableEntity  |
| TestString                           | multipart/form-data | application/json      | application/pdf          | 1             | ValidPayload     | UnprocessableEntity  |  
| 38f968d3-a8b9-49af-90d1              | multipart/form-data | application/json      | application/pdf          | 1             | ValidPayload     | UnprocessableEntity  |
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | application/json    |                       | application/pdf          | 0             | ValidPayload     | UnsupportedMediaType | 
| 38f968d3-a8b9-49af-90d1-47aed9cdab87 | text/xml            |                       | application/pdf          | 0             | XmlPayload       | UnsupportedMediaType |




Scenario Outline: Post preview template
  Given I set the input media type to "<MediaType>"
  When I call POST /"internal/preview" with payload "<Payload>"
  Then the response status should be "<ExpectedStatus>"
  And for successful responses the output media type should be "application/json"
  And for successful responses, the response should contain header Content-Version with value matching the app version

Examples:
| MediaType          | Payload          | ExpectedStatus       |
| application/json	 | ValidPayload     | OK                   |
| application/json	 | MalformedPayload | UnprocessableEntity  |
| text/xml			 | XmlPayload       | UnsupportedMediaType |
| application/json	 | EmptyPayload     | UnprocessableEntity  |
using AwesomeAssertions;
using IPO.Correspondence.BDDTests.Helpers;
using IPO.Correspondence.BDDTests.Mocks;
using Microsoft.AspNetCore.TestHost;
using Reqnroll;
using System.Net.Http.Headers;
using System.Text;

namespace IPO.Correspondence.BDDTests.StepDefinitions
{
    [Binding]
    public class CorrespondenceApiTestsStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public CorrespondenceApiTestsStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _server = TestStartup.GetTestServer();
            _client = _server.CreateClient();
        }

        [Given("I set the input media type to {string}")]
        public void GivenISetTheMediaTypeTo(string mediaType)
        {
            _scenarioContext["MediaType"] = mediaType;
        }

        [Given("I set the input content field media type to {string}")]
        public void GivenISetTheContentFieldMediaTypeTo(string mediaType)
        {
            _scenarioContext["ContentFieldMediaType"] = mediaType;
        }

        [Given("I set the file type to {string}")]
        public void GivenISetTheFileTypeTo(string fileType)
        {
            _scenarioContext["FileType"] = fileType;
        }

        [Given(@"I set the number of file attachments to (\d+)")]
        public void GivenISetTheNumberOfFileAttachmentsTo(int numberOfFiles)
        {
            _scenarioContext["NumberOfFiles"] = numberOfFiles;
        }

        [When(@"I call GET \/{string}")]
        public async Task WhenICallGET(string route)
        {
            var response = await _client.GetAsync($"/{route}");
            _scenarioContext["Response"] = response;
        }

        [When(@"I call GET \/{string} with query parameters {string} and {string}")]
        public async Task WhenICallGETWithQueryParametersAnd(string route, string dateFrom, string dateTo)
        {
            var response = await _client.GetAsync($"/{route}?{dateFrom}&{dateTo}");
            _scenarioContext["Response"] = response;
        }

        [When(@"I call POST \/{string} with payload {string}")]
        public async Task WhenICallPOSTWithPayload(string route, string payloadName)
        {
            var payload = MockPayloads.Payloads[payloadName];
            var content = new StringContent(payload, Encoding.UTF8, (string)_scenarioContext["MediaType"]);
            var response = await _client.PostAsync($"/{route}", content);
            _scenarioContext["Response"] = response;
        }

        [When(@"I call POST \/{string} with payload {string} and file attachment")]
        public async Task WhenICallPOSTWithPayloadAndFileAttachment(string route, string payloadName)
        {
            if ((string)_scenarioContext["MediaType"] == "multipart/form-data")
            {

                //Setup file/s to attach
                var fileType = _scenarioContext.Get<string>("FileType");
                int numberOfFiles = (int)_scenarioContext["NumberOfFiles"];
                var fileBytes = Encoding.UTF8.GetBytes("This is a test file.");
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(fileType);


                // Add file (loop to test that 0 and multiple files error)
                var multipartContent = new MultipartFormDataContent();
                for (int fileNumber = 0; fileNumber < (int)_scenarioContext["NumberOfFiles"]; fileNumber++)
                {
                    multipartContent.Add(fileContent, "file", "TestFileName");
                }


                //Add content field
                var contentField = MockPayloads.Payloads[payloadName];
                multipartContent.Add(new StringContent(contentField, Encoding.UTF8, (string)_scenarioContext["ContentFieldMediaType"]), "content");


                // Send request
                var response = await _client.PostAsync($"/{route}", multipartContent);
                _scenarioContext["Response"] = response;

            }
            else
            {
                await WhenICallPOSTWithPayload(route, payloadName);
            }
        }

        [Then("the response status should be {string}")]
        public void ThenTheResponseStatusShouldBe(string expectedStatus)
        {
            var response = (HttpResponseMessage)_scenarioContext["Response"];
            var actualStatus = response.StatusCode.ToString();

            actualStatus.Should().Be(expectedStatus);
        }

        [Then("for successful responses the output media type should be {string}")]
        public void ThenForSuccessfulResponsesTheMediaTypeShouldBe(string expectedMediaType)
        {
            var response = (HttpResponseMessage)_scenarioContext["Response"];
            var actualStatus = response.StatusCode.ToString();
            var successStatuses = new[] { "OK", "Created", "Accepted" };

            if (successStatuses.Contains(actualStatus))
            {
                var actualMediaType = response.Content.Headers.ContentType?.MediaType;
                actualMediaType.Should().Be(expectedMediaType);
            }
        }

        [Then("for successful responses, the response should contain header Content-Version with value matching the app version")]
        public void ThenForSuccessfulResponsesTheResponseShouldContainHeaderContent_VersionWithValueMatchingTheAppVersion()
        {
            var response = (HttpResponseMessage)_scenarioContext["Response"];
            var actualStatus = response.StatusCode.ToString();
            var successStatuses = new[] { "OK", "Created", "Accepted" };

            if (successStatuses.Contains(actualStatus))
            {
                response.Headers.TryGetValues("Content-Version", out var values);
                values.Should().NotBeNull();
                values.Should().Contain(TestStartup.Version!.FullVersion);
            }
        }
    }
}

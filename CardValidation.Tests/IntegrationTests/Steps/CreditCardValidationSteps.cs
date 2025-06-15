using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp;
using Reqnroll;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace CardValidation.Tests.Steps
{
    [Binding]
    [AllureNUnit]
    [AllureSuite("Credit Card Validation API Tests")]
    public class CreditCardValidationSteps
    {
        private readonly RestClient _client;
        private RestRequest? _request;
        private RestResponse? _response;

          public CreditCardValidationSteps()
        {
            // Get API URL from environment variable
            // When running in Docker, use host.docker.internal to reach host machine
            var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://localhost:7135";
            
            var options = new RestClientOptions(apiBaseUrl)
            {
                // Skip SSL certificate validation for development/testing
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            
            _client = new RestClient(options);
            
            Console.WriteLine($"API Base URL: {apiBaseUrl}");
        }


        [Given(@"this is test case ""(.*)""")]
        public void GivenThisIsTestCase(string testCaseName)
        {
            Console.WriteLine($"Running test case: {testCaseName}");
            // Optionally store in context for Allure or hooks
            ScenarioContext.Current["TestCaseName"] = testCaseName;
        }

        [Given(@"I prepare a credit card with:")]
        [AllureStep("Prepare credit card with provided data")]
        public void GivenIPrepareACreditCardWith(Table table)
        {
            if (table.Rows.Count == 0)
                throw new InvalidOperationException("Table must contain at least one data row");

            var row = table.Rows[0];

            var owner = row["Owner"]?.Trim() ?? string.Empty;
            var number = row["Number"]?.Trim() ?? string.Empty;
            var cvv = row["Cvv"]?.Trim() ?? string.Empty;
            var issueDate = row["IssueDate"]?.Trim() ?? string.Empty;

            _request = new RestRequest("/CardValidation/card/credit/validate", Method.Post)
                .AddJsonBody(new
                {
                    owner,
                    number,
                    cvv,
                    date = issueDate
                });
        }

        [When(@"I send the card to the validation API")]
        [AllureStep("Send credit card data to API")]
        public async Task WhenISendTheCardToTheValidationAPI()
        {
            if (_request == null)
                throw new InvalidOperationException("Request has not been initialized.");

            _response = await _client.ExecuteAsync(_request);
        }

        [Then(@"the response status should be (.*)")]
        [AllureStep("Validate HTTP response status")]
        public void ThenTheResponseStatusShouldBe(int expectedStatus)
        {
            if (_response == null)
                throw new InvalidOperationException("Response has not been received yet.");

            var actualStatus = (int)_response.StatusCode;
            Assert.That(actualStatus, Is.EqualTo(expectedStatus), 
                $"Expected HTTP {expectedStatus}, but got {actualStatus}. Response body: {_response.Content}");
        }

        [Then(@"the response body should contain ""(.*)""")]
        [AllureStep("Validate response body content")]
        public void ThenTheResponseBodyShouldContain(string expectedResult)
        {
            if (_response == null)
                throw new InvalidOperationException("Response has not been received yet.");

            Assert.That(_response.Content, Does.Contain(expectedResult).IgnoreCase,
                $"Expected response body to contain '{expectedResult}', but got: {_response.Content}");
        }
    }
}

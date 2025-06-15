using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Reqnroll;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace CardValidation.Tests.Steps
{
    [Binding]
    [AllureNUnit]
    [AllureSuite("Credit Card Validation API Tests")]
    public class CreditCardValidationSteps : IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private HttpRequestMessage? _request;
        private HttpResponseMessage? _response;
        private bool _disposed = false;

        public CreditCardValidationSteps()
        {
            // Create the test server factory
            _factory = new WebApplicationFactory<Program>();

            // Create HTTP client from the test server
            _client = _factory.CreateClient();
            
            Console.WriteLine("TestHost initialized for API testing");
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

            // Create the request payload
            var payload = new
            {
                owner,
                number,
                cvv,
                date = issueDate
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            
            _request = new HttpRequestMessage(HttpMethod.Post, "/CardValidation/card/credit/validate")
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            Console.WriteLine($"Prepared request with payload: {jsonContent}");
        }

        [When(@"I send the card to the validation API")]
        [AllureStep("Send credit card data to API")]
        public async Task WhenISendTheCardToTheValidationAPI()
        {
            if (_request == null)
                throw new InvalidOperationException("Request has not been initialized.");

            _response = await _client.SendAsync(_request);
            
            Console.WriteLine($"Received response with status: {_response.StatusCode}");
        }

        [Then(@"the response status should be (.*)")]
        [AllureStep("Validate HTTP response status")]
        public void ThenTheResponseStatusShouldBe(int expectedStatus)
        {
            if (_response == null)
                throw new InvalidOperationException("Response has not been received yet.");

            var actualStatus = (int)_response.StatusCode;
            var responseBody = _response.Content.ReadAsStringAsync().Result;
            
            Assert.That(actualStatus, Is.EqualTo(expectedStatus), 
                $"Expected HTTP {expectedStatus}, but got {actualStatus}. Response body: {responseBody}");
        }

        [Then(@"the response body should contain ""(.*)""")]
        [AllureStep("Validate response body content")]
        public async Task ThenTheResponseBodyShouldContain(string expectedResult)
        {
            if (_response == null)
                throw new InvalidOperationException("Response has not been received yet.");

            var responseContent = await _response.Content.ReadAsStringAsync();
            
            Assert.That(responseContent, Does.Contain(expectedResult).IgnoreCase,
                $"Expected response body to contain '{expectedResult}', but got: {responseContent}");
        }

        // Implement IDisposable to clean up resources
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _request?.Dispose();
                _response?.Dispose();
                _client?.Dispose();
                _factory?.Dispose();
                _disposed = true;
            }
        }
    }
}
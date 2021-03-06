// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing.xunit;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class InputFormatterTests : IClassFixture<MvcTestFixture<FormatterWebSite.Startup>>
    {
        public InputFormatterTests(MvcTestFixture<FormatterWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [ConditionalFact]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        public async Task CheckIfXmlInputFormatterIsBeingCalled()
        {
            // Arrange
            var sampleInputInt = 10;
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<DummyClass xmlns=\"http://schemas.datacontract.org/2004/07/FormatterWebSite\"><SampleInt>"
                + sampleInputInt.ToString() + "</SampleInt></DummyClass>";
            var content = new StringContent(input, Encoding.UTF8, "application/xml");

            // Act
            var response = await Client.PostAsync("http://localhost/Home/Index", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(sampleInputInt.ToString(), await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/json")]
        public async Task JsonInputFormatter_IsSelectedForJsonRequest(string requestContentType)
        {
            // Arrange
            var sampleInputInt = 10;
            var input = "{\"SampleInt\":10}";
            var content = new StringContent(input, Encoding.UTF8, requestContentType);

            // Act
            var response = await Client.PostAsync("http://localhost/Home/Index", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(sampleInputInt.ToString(), await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(null, true)]
        [InlineData("invalid", true)]
        [InlineData("application/custom", true)]
        [InlineData("image/jpg", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("invalid", false)]
        [InlineData("application/custom", false)]
        [InlineData("image/jpg", false)]
        public async Task ModelStateErrorValidation_NoInputFormatterFound_ForGivenContentType(
            string requestContentType,
            bool filterHandlesModelStateError)
        {
            // Arrange
            var actionName = filterHandlesModelStateError ? "ActionFilterHandlesError" : "ActionHandlesError";
            var expectedSource = filterHandlesModelStateError ? "filter" : "action";
            var input = "{\"SampleInt\":10}";
            var content = new StringContent(input);
            content.Headers.Clear();
            content.Headers.TryAddWithoutValidation("Content-Type", requestContentType);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/InputFormatter/" + actionName);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Content = content;
            var response = await Client.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FormatterWebSite.ErrorInfo>(responseBody);

            // Assert
            Assert.Equal(1, result.Errors.Count);
            Assert.Equal("Unsupported content type '" + requestContentType + "'.",
                         result.Errors[0]);
            Assert.Equal(actionName, result.ActionName);
            Assert.Equal("dummy", result.ParameterName);
            Assert.Equal(expectedSource, result.Source);
        }

        [Theory]
        [InlineData("application/json", "{\"SampleInt\":10}", 10)]
        [InlineData("application/json", "{}", 0)]
        public async Task JsonInputFormatter_IsModelStateValid_ForValidContentType(
            string requestContentType,
            string jsonInput,
            int expectedSampleIntValue)
        {
            // Arrange
            var content = new StringContent(jsonInput, Encoding.UTF8, requestContentType);

            // Act
            var response = await Client.PostAsync("http://localhost/JsonFormatter/ReturnInput/", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedSampleIntValue.ToString(), responseBody);
        }

        [Theory]
        [InlineData("\"I'm a JSON string!\"")]
        [InlineData("true")]
        [InlineData("\"\"")] // Empty string
        public async Task JsonInputFormatter_ReturnsDefaultValue_ForValueTypes(string input)
        {
            // Arrange
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("http://localhost/JsonFormatter/ValueTypeAsBody/", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("0", responseBody);
        }

        [Fact]
        public async Task JsonInputFormatter_ReadsPrimitiveTypes()
        {
            // Arrange
            var expected = "1773";
            var content = new StringContent(expected, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("http://localhost/JsonFormatter/ValueTypeAsBody/", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expected, responseBody);
        }

        [Theory]
        [InlineData("{\"SampleInt\":10}")]
        [InlineData("{}")]
        [InlineData("")]
        public async Task JsonInputFormatter_IsModelStateInvalid_ForEmptyContentType(string jsonInput)
        {
            // Arrange
            var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");
            content.Headers.Clear();

            // Act
            var response = await Client.PostAsync("http://localhost/JsonFormatter/ReturnInput/", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("application/json", "{\"SampleInt\":10}", 10)]
        [InlineData("application/json", "{}", 0)]
        public async Task JsonInputFormatter_IsModelStateValid_ForTransferEncodingChunk(
            string requestContentType,
            string jsonInput,
            int expectedSampleIntValue)
        {
            // Arrange
            var content = new StringContent(jsonInput, Encoding.UTF8, requestContentType);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/JsonFormatter/ReturnInput/");
            request.Headers.TransferEncodingChunked = true;
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedSampleIntValue.ToString(), responseBody);
        }

        [Theory]
        [InlineData("utf-8")]
        [InlineData("unicode")]
        public async Task CustomFormatter_IsSelected_ForSupportedContentTypeAndEncoding(string encoding)
        {
            // Arrange
            var content = new StringContent("Test Content", Encoding.GetEncoding(encoding), "text/plain");

            // Act
            var response = await Client.PostAsync("http://localhost/InputFormatter/ReturnInput/", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Test Content", responseBody);
        }

        [Theory]
        [InlineData("image/png")]
        [InlineData("image/jpeg")]
        public async Task CustomFormatter_NotSelected_ForUnsupportedContentType(string contentType)
        {
            // Arrange
            var content = new StringContent("Test Content", Encoding.UTF8, contentType);

            // Act
            var response = await Client.PostAsync("http://localhost/InputFormatter/ReturnInput/", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
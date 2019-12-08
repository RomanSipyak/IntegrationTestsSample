using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using IntegrationTestsSample.Data;
using IntegrationTestsSample.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using RazorPagesProject.Tests.Helpers;

namespace RazorPagesProject.Tests.Integration_Tests
{
    public class IndexPageTests : IClassFixture<CustomWebApplicationFactory<IntegrationTestsSample.Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<IntegrationTestsSample.Startup> _factory;

        public IndexPageTests(CustomWebApplicationFactory<IntegrationTestsSample.Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Post_DeleteAllMessagesHandler_ReturnsRedirectToRoot()
        {
            // Arrange
            var defaultPage = await _client.GetAsync("/");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);

            //Act
            var response = await _client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='messages']"),
                (IHtmlButtonElement)content.QuerySelector("button[id='deleteAllBtn']"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }
        [Fact]
        public async Task Btn_Add_Message_Return_Text_When_NotNull_String()
        {
            // Arrange
            var defaultPage = await _client.GetAsync("/");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            var messagesContainer = content.QuerySelector("#messages").QuerySelectorAll("li");

            bool isquote = false;
            foreach (var li in messagesContainer)
            {
                if (li.TextContent.Contains("Helol"))
                {
                    isquote = true;
                }
            }
            Assert.False(isquote);
            //Act
            var response = await _client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='addMessage']"),
                (IHtmlButtonElement)content.QuerySelector("button[id='addMessageBtn']"),
                new Dictionary<string, string> { ["Message.Text"] = "Helol" });

            defaultPage = await _client.GetAsync("/");
            content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            messagesContainer = content.QuerySelector("#messages").QuerySelectorAll("li");
            foreach (var li in messagesContainer)
            {
                if (li.TextContent.Contains("Helol"))
                {
                    isquote = true;
                }
            }
            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.True(isquote);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }
        [Fact]
        public async Task Btn_Analize_WhenMessageNotNull()
        {
            // Arrange
            var defaultPage = await _client.GetAsync("/");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            var messagesContainer = content.QuerySelector("#analyze").QuerySelectorAll("div.form-group");

            bool isquote = false;
            foreach (var li in messagesContainer)
            {
                if (li.TextContent.Contains("The average message length"))
                {
                    isquote = true;
                }
            }
            Assert.False(isquote);
            //Act
            var response = await _client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='analyze']"),
                (IHtmlButtonElement)content.QuerySelector("button[id='analyzeBtn']"));

            defaultPage = await _client.GetAsync("/");
            content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            messagesContainer = content.QuerySelector("#analyze").QuerySelectorAll("div.form-group");
            foreach (var li in messagesContainer)
            {
                if (li.TextContent.Contains("The average message length"))
                {
                    isquote = true;
                }
            }
            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.True(isquote);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }
        [Fact]
        public async Task Btn_Analize_AfterDeleteAllMessages()
        {
            // Arrange
            var defaultPage = await _client.GetAsync("/");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            var messagesContainer = content.QuerySelector("#analyze").QuerySelectorAll("div.form-group");

            bool isquote = false;
            foreach (var li in messagesContainer)
            {
                if (li.TextContent.Contains("There are no messages to analyze."))
                {
                    isquote = true;
                }
            }
            Assert.False(isquote);
            //Act
            await _client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='messages']"),
                (IHtmlButtonElement)content.QuerySelector("button[id='deleteAllBtn']"));

            var response = await _client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='analyze']"),
                (IHtmlButtonElement)content.QuerySelector("button[id='analyzeBtn']"));

            defaultPage = await _client.GetAsync("/");
            content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            messagesContainer = content.QuerySelector("#analyze").QuerySelectorAll("div.form-group");
            foreach (var li in messagesContainer)
            {
                if (li.TextContent.Contains("There are no messages to analyze."))
                {
                    isquote = true;
                }
            }
            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.True(isquote);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }
        [Fact]
        public async Task Btn_Add_Message_Return_Text_When_String_Length_Is_Over_200()
        {
            // Arrange
            var defaultPage = await _client.GetAsync("/");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);

            //Act
            var response = await _client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='addMessage']"),
                (IHtmlButtonElement)content.QuerySelector("button[id='addMessageBtn']"),
                new Dictionary<string, string> { ["Message.Text"] = new string('a', 201) });

            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(defaultPage.Headers.Location);
        }
        [Fact]
        public async Task Post_DeleteMessageHandler_ReturnsRedirectToRoot()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceProvider = services.BuildServiceProvider();

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices
                            .GetRequiredService<ApplicationDbContext>();
                        var logger = scopedServices
                            .GetRequiredService<ILogger<IndexPageTests>>();

                        try
                        {
                            Utilities.ReinitializeDbForTests(db);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred seeding " +
                                "the database with test messages. Error: {Message}",
                                ex.Message);
                        }
                    }
                });
            })
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

            var defaultPage = await client.GetAsync("/");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);

            //Act
            var response = await client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='messages']"),
                (IHtmlButtonElement)content.QuerySelector("form[id='messages']")
                    .QuerySelector("div[class='panel-body']")
                    .QuerySelector("button"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }

        public class TestQuoteService : IQuoteService
        {
            public Task<string> GenerateQuote()
            {
                return Task.FromResult<string>(
                    "Something's interfering with time, Mr. Scarman, " +
                    "and time is my business.");
            }
        }

        [Fact]
        public async Task Get_QuoteService_ProvidesQuoteInPage()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<IQuoteService, TestQuoteService>();
                });
            })
                .CreateClient();

            //Act
            var defaultPage = await client.GetAsync("/");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);
            var quoteElement = content.QuerySelector("#quote");

            // Assert
            Assert.Equal("Something's interfering with time, Mr. Scarman, " +
                "and time is my business.", quoteElement.Attributes["value"].Value);
        }
    }
}
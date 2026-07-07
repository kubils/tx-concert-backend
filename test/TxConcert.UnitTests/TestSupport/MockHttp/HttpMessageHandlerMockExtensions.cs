using System.Linq.Expressions;
using System.Net;
using System.Text;
using Moq;
using Moq.Protected;

namespace TxConcert.UnitTests.TestSupport.MockHttp;

public static class HttpMessageHandlerMockExtensions
{
    public static Mock<HttpMessageHandler> SetupJsonResponse(
        this Mock<HttpMessageHandler> handler,
        HttpStatusCode status,
        string jsonBody,
        Expression<Func<HttpRequestMessage, bool>>? requestMatcher = null)
    {
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                requestMatcher is null
                    ? ItExpr.IsAny<HttpRequestMessage>()
                    : ItExpr.Is(requestMatcher),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(status)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            });
        return handler;
    }

    public static Mock<HttpMessageHandler> SetupSequentialJsonResponses(
        this Mock<HttpMessageHandler> handler,
        params (HttpStatusCode Status, string JsonBody)[] responses)
    {
        var setup = handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        foreach ((HttpStatusCode status, string body) in responses)
        {
            setup.ReturnsAsync(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }

        return handler;
    }

    public static HttpClient ToClient(this Mock<HttpMessageHandler> handler, string? baseAddress = null)
    {
        var client = new HttpClient(handler.Object);
        if (baseAddress is not null)
            client.BaseAddress = new Uri(baseAddress);
        return client;
    }
}

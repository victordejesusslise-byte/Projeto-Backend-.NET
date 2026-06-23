using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using UsuariosAPI.API.Middlewares;
using UsuariosAPI.Application.DTOs.Responses;
using Xunit;

namespace UsuariosAPI.UnitTests.Middlewares;

public class ExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task FalhaInterna_Development_RetornaDebugETraceId()
    {
        var response = await ExecutarMiddlewareAsync("Development");

        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("trace-teste", response.Body.TraceId);
        Assert.NotNull(response.Body.Debug);
        Assert.Equal("falha de teste", response.Body.Debug!.Mensagem);
    }

    [Fact]
    public async Task FalhaInterna_Production_NaoRetornaDebug()
    {
        var response = await ExecutarMiddlewareAsync("Production");

        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("trace-teste", response.Body.TraceId);
        Assert.Null(response.Body.Debug);
        Assert.DoesNotContain("falha de teste", response.Json);
    }

    private static async Task<(int StatusCode, ApiResponse<object> Body, string Json)>
        ExecutarMiddlewareAsync(string environmentName)
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-teste"
        };
        context.Response.Body = new MemoryStream();

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns(environmentName);

        var middleware = new ExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("falha de teste"),
            Mock.Of<ILogger<ExceptionHandlerMiddleware>>(),
            environment.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        var body = JsonSerializer.Deserialize<ApiResponse<object>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return (context.Response.StatusCode, body!, json);
    }
}

using System.Text.Json;

using AzureOpenAIProxy.AppHost.Tests.Fixtures;

using FluentAssertions;

using IdentityModel.Client;


namespace AzureOpenAIProxy.AppHost.Tests.ApiApp.Endpoints;

public class GetDeploymentModelsOpenApiTests(AspireAppHostFixture host) : IClassFixture<AspireAppHostFixture>
{
    [Fact]
    public async Task Given_Resource_When_Invoked_Endpoint_Then_It_Should_Return_Path()
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var apiDocument = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = apiDocument!.RootElement.GetProperty("paths")
                                             .TryGetProperty("/events/{eventId}/deployment-models", out var property) ? property : default;
        result.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task Given_Resource_When_Invoked_Endpoint_Then_It_Should_Return_Verb()
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var apiDocument = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = apiDocument!.RootElement.GetProperty("paths")
                                             .GetProperty("/events/{eventId}/deployment-models")
                                             .TryGetProperty("get", out var property) ? property : default;
        result.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Theory]
    [InlineData("events")]
    public async Task Given_Resource_When_Invoked_Endpoint_Then_It_Should_Return_Tags(string tag)
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var apiDocument = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = apiDocument!.RootElement.GetProperty("paths")
                                             .GetProperty("/events/{eventId}/deployment-models")
                                             .GetProperty("get")
                                             .TryGetProperty("tags", out var property) ? property : default;
        result.ValueKind.Should().Be(JsonValueKind.Array);
        result.EnumerateArray().Select(p => p.GetString()).Should().Contain(tag);
    }

    [Theory]
    [InlineData("summary")]
    [InlineData("description")]
    [InlineData("operationId")]
    public async Task Given_Resource_When_Invoked_Endpoint_Then_It_Should_Return_Value(string attribute)
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var apiDocument = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = apiDocument!.RootElement.GetProperty("paths")
                                             .GetProperty("/events/{eventId}/deployment-models")
                                             .GetProperty("get")
                                             .TryGetProperty(attribute, out var property) ? property : default;
        result.ValueKind.Should().Be(JsonValueKind.String);
    }


    [Theory]
    [InlineData("eventId")]
    public async Task Given_Resource_When_Invoked_Endpoint_Then_It_Should_Return_Path_Parameter(string name)
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var openapi = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = openapi!.RootElement.GetProperty("paths")
                                         .GetProperty("/events/{eventId}/deployment-models")
                                         .GetProperty("get")
                                         .GetProperty("parameters")
                                         .EnumerateArray()
                                         .Where(p => p.GetProperty("in").GetString() == "path")
                                         .Select(p => p.GetProperty("name").ToString());
        result.Should().Contain(name);
    }

    [Theory]
    [InlineData("responses")]
    public async Task Given_Resource_When_Invoked_Endpoint_Then_It_Should_Return_Object(string attribute)
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var apiDocument = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = apiDocument!.RootElement.GetProperty("paths")
                                             .GetProperty("/events/{eventId}/deployment-models")
                                             .GetProperty("get")
                                             .TryGetProperty(attribute, out var property) ? property : default;
        result.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Theory]
    [InlineData("200")]
    [InlineData("401")]
    [InlineData("404")]
    [InlineData("500")]
    public async Task Given_Resource_When_Invoked_Endpoint_Then_It_Should_Return_Response(string attribute)
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var apiDocument = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = apiDocument!.RootElement.GetProperty("paths")
                                             .GetProperty("/events/{eventId}/deployment-models")
                                             .GetProperty("get")
                                             .GetProperty("responses")
                                             .TryGetProperty(attribute, out var property) ? property : default;
        result.ValueKind.Should().Be(JsonValueKind.Object);
    }

    public static IEnumerable<object[]> DeploymentModelAttributeData =>
    [
        ["name", true, "string"]
    ];

    [Theory]
    [MemberData(nameof(DeploymentModelAttributeData))]
    public async Task Given_DeploymentModel_When_Invoked_Endpoint_Then_It_Should_Return_Type(string attribute, bool isRequired, string type)
    {
        // Arrange
        using var httpClient = host.App!.CreateHttpClient("apiapp");
        await host.ResourceNotificationService.WaitForResourceAsync("apiapp", KnownResourceStates.Running)
                                              .WaitAsync(TimeSpan.FromSeconds(30));

        // Act
        var json = await httpClient.GetStringAsync("/swagger/v1.0.0/swagger.json");
        var openapi = JsonSerializer.Deserialize<JsonDocument>(json);

        // Assert
        var result = openapi!.RootElement.GetProperty("components")
                                         .GetProperty("schemas")
                                         .GetProperty("DeploymentModelDetails")
                                         .GetProperty("properties")
                                         .GetProperty(attribute);

        result.TryGetString("type").Should().Be(type);

        if (isRequired)
        {
            var requiredProperties = openapi.RootElement.GetProperty("components")
                                                        .GetProperty("schemas")
                                                        .GetProperty("DeploymentModelDetails")
                                                        .GetProperty("required");
            requiredProperties.EnumerateArray().Any(p => p.GetString() == attribute).Should().BeTrue();
        }
    }
}
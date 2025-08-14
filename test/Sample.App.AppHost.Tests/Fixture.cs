using Dapr.Client;

namespace Sample.App.AppHost.Tests;

public class Fixture
{
    public async Task Test(string webResourceName, Func<HttpClient, Task> f)
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Sample_App_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        // Act
        var daprHttpClient = app.CreateHttpClient($"{webResourceName}-dapr-cli");

        var db = new DaprClientBuilder().UseHttpEndpoint(daprHttpClient.BaseAddress!.ToString());
        var dapr = db.Build();

        var httpClient = app.CreateHttpClient(webResourceName);
        await resourceNotificationService.WaitForResourceAsync(webResourceName, KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

        // Assert
        await f(httpClient);
    }
}
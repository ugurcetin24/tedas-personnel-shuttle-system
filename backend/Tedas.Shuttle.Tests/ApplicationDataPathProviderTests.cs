using Tedas.Shuttle.Infrastructure;

namespace Tedas.Shuttle.Tests;

public sealed class ApplicationDataPathProviderTests
{
    [Fact]
    public void DatabasePath_UsesExpectedApplicationDataDirectory()
    {
        var provider = new ApplicationDataPathProvider();

        Assert.EndsWith(
            Path.Combine("TedasPersonnelShuttleSystem", "tedas-personnel-shuttle.db"),
            provider.DatabasePath);
        Assert.EndsWith(
            Path.Combine("TedasPersonnelShuttleSystem", "logs"),
            provider.LogsDirectory);
        Assert.Contains("Data Source=", provider.DatabaseConnectionString);
    }
}

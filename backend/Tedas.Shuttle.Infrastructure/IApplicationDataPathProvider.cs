namespace Tedas.Shuttle.Infrastructure;

public interface IApplicationDataPathProvider
{
    string ApplicationDataDirectory { get; }

    string DatabasePath { get; }

    string LogsDirectory { get; }

    string DatabaseConnectionString { get; }

    void EnsureDirectoriesExist();
}

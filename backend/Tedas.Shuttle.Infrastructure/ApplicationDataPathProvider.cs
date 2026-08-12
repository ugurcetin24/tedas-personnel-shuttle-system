using Microsoft.Data.Sqlite;

namespace Tedas.Shuttle.Infrastructure;

public sealed class ApplicationDataPathProvider : IApplicationDataPathProvider
{
    private const string ApplicationDirectoryName = "TedasPersonnelShuttleSystem";
    private const string DatabaseFileName = "tedas-personnel-shuttle.db";

    public string ApplicationDataDirectory { get; }

    public string DatabasePath => Path.Combine(ApplicationDataDirectory, DatabaseFileName);

    public string LogsDirectory => Path.Combine(ApplicationDataDirectory, "logs");

    public string DatabaseConnectionString
    {
        get
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = DatabasePath
            };

            return connectionStringBuilder.ToString();
        }
    }

    public ApplicationDataPathProvider()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        ApplicationDataDirectory = Path.Combine(localAppData, ApplicationDirectoryName);
    }

    public void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(ApplicationDataDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }
}

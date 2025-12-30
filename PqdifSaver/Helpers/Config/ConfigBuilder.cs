using Microsoft.Extensions.Configuration;

/// <summary>
/// Singleton configuration builder for loading and providing application connection strings.
/// Loads settings from appsettings.json at startup.
/// </summary>
public class ConfigBuilder : ConfigurationBuilder
{
    private static readonly Lazy<ConfigBuilder> _instance =
        new Lazy<ConfigBuilder>(() => new ConfigBuilder());

    /// <summary>
    /// Gets the singleton instance of the ConfigBuilder.
    /// </summary>
    public static ConfigBuilder Instance => _instance.Value;

    /// <summary>
    /// The SQL Server connection string loaded from configuration.
    /// </summary>
    public readonly string ConnectionString;
    /// <summary>
    /// The DuckDB connection string loaded from configuration.
    /// </summary>
    public readonly string DuckDBConnectionString;

    /// <summary>
    /// Initializes the configuration builder and loads connection strings from appsettings.json.
    /// </summary>
    private ConfigBuilder() : base()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        this.AddJsonFile(path, optional: false, reloadOnChange: true);
        var config = this.Build();
        ConnectionString = config.GetConnectionString("DefaultConnection");
        DuckDBConnectionString = config.GetConnectionString("DuckDBConnection");
    }
}

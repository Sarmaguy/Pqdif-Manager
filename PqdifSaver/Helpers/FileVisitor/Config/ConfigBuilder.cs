using Microsoft.Extensions.Configuration;

public class ConfigBuilder : ConfigurationBuilder
{
    private static readonly Lazy<ConfigBuilder> _instance =
        new Lazy<ConfigBuilder>(() => new ConfigBuilder());

    public static ConfigBuilder Instance => _instance.Value;

    public readonly string ConnectionString;

    private ConfigBuilder() : base()
    {
        this.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        var config = this.Build();
        ConnectionString = config.GetConnectionString("DefaultConnection");
    }
}

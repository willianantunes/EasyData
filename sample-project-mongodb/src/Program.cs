using CliFx;
using Serilog;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var configuration = BuildConfiguration();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Information("Starting up the application.");

        try
        {
            return await new CliApplicationBuilder()
                .SetExecutableName("SampleProjectMongo")
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    public static IConfiguration BuildConfiguration()
    {
        var solutionSettings = Path.Combine(Directory.GetCurrentDirectory(), "..", "appsettings.json");
        if (!File.Exists(solutionSettings))
            solutionSettings = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        return new ConfigurationBuilder()
            .AddJsonFile(solutionSettings, optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }
}

// ReSharper disable HeapView.BoxingAllocation
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable UnusedVariable
// ReSharper disable InvertIf
// ReSharper disable MemberCanBePrivate.Global

namespace Pricing;

[ExcludeFromCodeCoverage]
public class App : IHostedService
{
    public App(ILogger<App>             logger
             , IHostApplicationLifetime appLifetime)
    {
        Logger      = logger;
        AppLifetime = appLifetime;
    }

    public void RunWithArgs(DirectoryInfo output, FileInfo[] config)
    {
        try
        {
            List<Results> results = [];

            EnsureOutputDirectoriesExists(output);
            var configFiles = LoadConfigFiles(config);

            foreach (var configFile in configFiles)
            {
                var result = new Results
                             {
                                 OutputPath     = output.FullName
                               , ConfigName     = configFile.Name
                               , ConfigFullPath = configFile.FileInfo.FullName
                               , TextFullPath   = Path.Combine(output.FullName, "text", $"{configFile.Name}.txt")
                               , JsonFullPath   = Path.Combine(output.FullName, "json", $"{configFile.Name}.json")
                               , TextContent    = configFile.Display(new TextDisplayStrategy(4))
                               , JsonContent    = configFile.Display(new JsonDisplayStrategy())
                               , Price          = configFile.Price
                             };

                File.WriteAllText(result.TextFullPath, result.TextContent);
                File.WriteAllText(result.JsonFullPath, result.JsonContent);

                results.Add(result);
            }

            var maxNameLength  = results.Max(static r => r.ConfigName.Length);
            var maxPriceLength = results.Max(static r => r.Price.Year.Display.Length);

            Console.WriteLine();

            results.ForEach(result =>
                            {
                                var name  = result.ConfigName.PadRight(maxNameLength);
                                var price = result.Price.Year.Display.PadLeft(maxPriceLength);
                                Console.WriteLine($"{name} = ${price}    {result.ConfigFullPath}");
                            });

            var directoryInfoVisitor = new DirectoryInfoVisitor();
            var tree                 = directoryInfoVisitor.Tree(output);

            Console.WriteLine($"\n{tree}");
        }
        catch (FileNotFoundException e)
        {
            Logger.LogError("Error loading configuration files: {Message}.", e.Message);
            ExitCode = 1;
            return;
        }
        catch (IOException e)
        {
            Logger.LogError(e, "Error creating output directory: {Message}.", e.Message);
            ExitCode = 2;
            return;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unhandled exception!");
            ExitCode = 3;
            return;
        }

        ExitCode = 0;
    }

    #region Properties

    private ILogger<App>             Logger      { get; }
    private IHostApplicationLifetime AppLifetime { get; }
    private int                      ExitCode    { get; set; }

    #endregion

    #region Start/Stop/Run

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Starting with arguments: {Args}", string.Join(" ", Environment.GetCommandLineArgs()));

        AppLifetime.ApplicationStarted.Register(() => _ = Task.Run(RunAsync, cancellationToken));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Exiting with return code: {ExitCode}", ExitCode);
        Environment.ExitCode = ExitCode;
        return Task.CompletedTask;
    }

    private async Task? RunAsync()
    {
        try
        {
            var rootCommand = new RootCommand
                              {
                                  new Option<string>("--output", "The output directory")
                                  {
                                      IsRequired = true
                                  }
                                , new Option<string[]>("--config", "A configuration file")
                                  {
                                      IsRequired                     = true
                                    , AllowMultipleArgumentsPerToken = true
                                  }
                              };

            rootCommand.Description = "ECS Fargate Pricing Calculator";
            rootCommand.Handler     = CommandHandler.Create<string, string[]>(HandleCommandLineArgs);

            // Skip the first argument (executable path)
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            _ = await rootCommand.InvokeAsync(args).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Unhandled exception!");
            ExitCode = 1;
        }
        finally
        {
            AppLifetime.StopApplication();
        }
    }

    #endregion

    #region Helper

    private void HandleCommandLineArgs(string output, string[] config)
    {
        var filesPath = Path.Combine(AppContext.BaseDirectory, "Files");

        var baseDir = Directory.Exists(filesPath)
                          ? AppContext.BaseDirectory                                               // published or copied with Files/
                          : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")); // dev context

        var outputPath  = ResolvePath(output, baseDir);
        var configFiles = config.Select(c => new FileInfo(ResolvePath(c, baseDir))).ToArray();

        RunWithArgs(new (outputPath), configFiles);
    }

    private string ResolvePath(string path, string baseDir)
    {
        return Path.IsPathRooted(path)
                   ? path
                   : Path.GetFullPath(Path.Combine(baseDir, path.TrimStart('.', '/', '\\')));
    }

    public static List<Config> LoadConfigFiles(FileInfo[] configFileInfoArray)
    {
        List<Config> configs = [];

        foreach (var fileInfo in configFileInfoArray)
        {
            var config = LoadConfigFile(fileInfo);
            if (config == null)
            {
                throw new FileNotFoundException($"'{fileInfo.FullName}' not found.");
            }

            configs.Add(config);
        }

        return configs;
    }

    public static Config? LoadConfigFile(FileInfo fileInfo)
    {
        if (!fileInfo.Exists)
        {
            return null;
        }

        var yamlContent = File.ReadAllText(fileInfo.FullName);
        var deserializer = new DeserializerBuilder()
                          .WithNamingConvention(CamelCaseNamingConvention.Instance)
                          .WithTypeConverter(new CurrencyYamlTypeConverter())
                          .Build();

        var config = deserializer.Deserialize<Config>(yamlContent);

        config.FileInfo = fileInfo;

        var onDeserializedVisitor = new ConfigOnDeserializedVisitor(config);
        var calculatePriceVisitor = new CalculatePriceVisitor();
        foreach (var cluster in config.Clusters)
        {
            onDeserializedVisitor.VisitCluster(cluster);
            calculatePriceVisitor.VisitCluster(cluster);
        }

        var totalDailyPrice = config.Clusters.Sum(static cluster => cluster.TotalPrice.Day.Value);
        config.Price = new (totalDailyPrice);

        return config;
    }

    public static void EnsureOutputDirectoriesExists(DirectoryInfo output)
    {
        Directory.CreateDirectory(output.FullName);

        output.CreateSubdirectory("json");
        output.CreateSubdirectory("text");
    }

    #endregion
}

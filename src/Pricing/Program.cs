var assemblyName    = Assembly.GetExecutingAssembly().GetName().Name;
var appsettingsName = $"{assemblyName}.appsettings.json";

await Host.CreateDefaultBuilder(args)
          .ConfigureHostConfiguration(configHost =>
                                      {
                                          configHost.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                                          configHost.AddJsonFile(appsettingsName, true);
                                          configHost.AddEnvironmentVariables($"{assemblyName}_");
                                          configHost.AddCommandLine(args);
                                      })
          .ConfigureLogging(static logging =>
                            {
                                logging.AddConsole();
                                logging.AddDebug();
                            })
          .ConfigureServices(static services => services.AddHostedService<App>())
          .RunConsoleAsync()
          .ConfigureAwait(false);

//----------------------------------------------------------------------------
//
//   To Run:
//
//   > dotnet run --output ./Files/Output --config ./Files/Input/SavingsPlanScaling.yml --config ./Files/Input/OnDemandScaling.yml --config ./Files/Input/OnDemand24x7.yml
//
//   SavingsPlanScaling = $27,041.24    C:\krk\research\aws\src\Pricing\Files\Input\SavingsPlanScaling.yml
//   OnDemandScaling    = $32,269.63    C:\krk\research\aws\src\Pricing\Files\Input\OnDemandScaling.yml
//   OnDemand24x7       = $42,890.02    C:\krk\research\aws\src\Pricing\Files\Input\OnDemand24x7.yml
//
//   C:\krk\research\aws\src\Pricing\Files\Output
//   |-- json
//   |   |-- OnDemand24x7.json
//   |   |-- OnDemandScaling.json
//   |   `-- SavingsPlanScaling.json
//   `-- text
//       |-- OnDemand24x7.txt
//       |-- OnDemandScaling.txt
//       `-- SavingsPlanScaling.txt
//
//----------------------------------------------------------------------------
//
//   To Publish:
//
//   > dotnet publish -c Release -o C:\krk\publish\aws-pricing
//
//   C:\krk\publish\aws-pricing
//   |-- Files
//   |   |-- Input
//   |   |   |-- OnDemand24x7.yml
//   |   |   |-- OnDemandScaling.yml
//   |   |   `-- SavingsPlanScaling.yml
//   |   `-- Output
//   |       |-- json
//   |       `-- text
//   |-- aws-pricing.appsettings.json
//   `-- aws-pricing.exe
//
//----------------------------------------------------------------------------
//
//   To Run Published:
//
//   > aws-pricing.exe --output ./Files/Output --config ./Files/Input/SavingsPlanScaling.yml --config ./Files/Input/OnDemandScaling.yml --config ./Files/Input/OnDemand24x7.yml
//
//   SavingsPlanScaling = $27,041.24    C:\krk\publish\aws-pricing\Files\Input\SavingsPlanScaling.yml
//   OnDemandScaling    = $32,269.63    C:\krk\publish\aws-pricing\Files\Input\OnDemandScaling.yml
//   OnDemand24x7       = $42,890.02    C:\krk\publish\aws-pricing\Files\Input\OnDemand24x7.yml
//
//   C:\krk\publish\aws-pricing\Files\Output
//   |-- json
//   |   |-- OnDemand24x7.json
//   |   |-- OnDemandScaling.json
//   |   `-- SavingsPlanScaling.json
//   `-- text
//       |-- OnDemand24x7.txt
//       |-- OnDemandScaling.txt
//       `-- SavingsPlanScaling.txt
//
//----------------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using OcrWorker.Runtime;
using OcrWorker.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(c =>
    {
        c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        c.AddEnvironmentVariables(); // docker-compose overrides
    })
    .ConfigureServices((ctx, s) =>
    {
        s.AddHttpClient(); // falls benötigt
        s.AddSingleton<IRabbitConsumer, RabbitConsumer>();
        s.AddSingleton<IObjectStore, MinioStore>();
        s.AddSingleton<IOcrEngine, TesseractOcr>();
        s.AddHostedService<OcrBackgroundService>();
        s.AddLogging();
    })
    .RunConsoleAsync();

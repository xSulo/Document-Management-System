using GenAIWorker;
using GenAIWorker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHttpClient();

        services.AddSingleton<IGenAIClient, GeminiClient>();
        services.AddSingleton<RabbitPublisher>();

        services.AddHostedService<Worker>();
    })
    .RunConsoleAsync();

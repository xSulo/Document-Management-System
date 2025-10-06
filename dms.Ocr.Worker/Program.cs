using dms.Ocr.Worker;
using dms.Ocr.Worker.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<OcrConsumer>();

var host = builder.Build();
host.Run();

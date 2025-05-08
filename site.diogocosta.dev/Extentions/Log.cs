using Serilog;
using Serilog.Events;

namespace site.diogocosta.dev.Extentions;

public static class ConfigLog
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        // Define o nível mínimo de log baseado no ambiente
        var minimumLevel = builder.Environment.IsDevelopment() 
            ? LogEventLevel.Information 
            : LogEventLevel.Warning;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Seq("https://seq.diogocosta.dev", apiKey: "OyTLRUtv96SYrvz3pyiQ")
            .CreateLogger();

        builder.Host.UseSerilog(Log.Logger);
    }
}
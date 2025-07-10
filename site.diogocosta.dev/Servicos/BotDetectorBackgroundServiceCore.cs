using site.diogocosta.dev.Servicos.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace site.diogocosta.dev.Servicos;

/// <summary>
/// Background service para detecção automática de bots usando AntiSpam.Core
/// </summary>
public class BotDetectorBackgroundServiceCore : BackgroundService
{
    private readonly ILogger<BotDetectorBackgroundServiceCore> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _interval;

    public BotDetectorBackgroundServiceCore(
        ILogger<BotDetectorBackgroundServiceCore> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        
        // Configurar intervalo de execução (padrão: 5 minutos)
        var intervalMinutes = _configuration.GetValue<int>("BotDetector:AnalysisIntervalMinutes", 5);
        _interval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BotDetectorBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AnalyzeAndCreateRulesAsync();
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Serviço sendo interrompido
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise de detecção de bots");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Aguardar antes de tentar novamente
            }
        }

        _logger.LogInformation("BotDetectorBackgroundService finalizado");
    }

    private async Task AnalyzeAndCreateRulesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var antiSpamService = scope.ServiceProvider.GetRequiredService<IAntiSpamService>();

        _logger.LogDebug("Iniciando análise de detecção de bots");

        try
        {
            // Usar o serviço original para análise
            var originalBotDetector = scope.ServiceProvider.GetService<BotDetectorBackgroundService>();
            if (originalBotDetector != null)
            {
                // Delegar para o serviço original que já tem toda a lógica
                var analyzeMethod = typeof(BotDetectorBackgroundService)
                    .GetMethod("AnalyzeAndCreateRulesAsync", 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                
                if (analyzeMethod != null)
                {
                    await (Task)analyzeMethod.Invoke(originalBotDetector, null)!;
                }
            }

            _logger.LogDebug("Análise de detecção de bots concluída");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante análise automática de bots");
        }
    }
}

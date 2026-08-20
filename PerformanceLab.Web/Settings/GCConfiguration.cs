using System.Runtime;

public static class GCConfiguration
{
    private const string CONFIG_SECTION = "RuntimeOptions:GCSettings";
    private const string SERVER_GC_KEY = "ServerGC";
    private const string LATENCY_MODE_KEY = "LatencyMode";

    public static GCOptions ConfigureGC(
        this IConfiguration configuration,
        IServiceCollection services)
    {
        // 1. Ler configurações
        GCOptions gcOptions = LoadGCOptions(configuration);

        // 2. Aplicar configurações do GC no runtime
        ApplyGCConfiguration(gcOptions);

        // 3. Registrar para uso posterior via DI
        services.Configure<GCOptions>(
            configuration.GetSection(CONFIG_SECTION)
        );

        // 4. Registrar também como singleton para acesso direto (opcional)
        services.AddSingleton(gcOptions);

        return gcOptions;
    }

    private static void ApplyGCConfiguration(GCOptions options)
    {
        // Configurar Server GC
        if (options.IsServerGC)
        {
            AppContext.SetSwitch("System.GC.Server", true);
        }
        else
        {
            AppContext.SetSwitch("System.GC.Server", false);
        }

        // Configurar Concurrent GC (opcional)
        // AppContext.SetSwitch("System.GC.Concurrent", options.ConcurrentGC);

        // Configurar modo de latência (se suportado)
        if (!string.IsNullOrEmpty(options.LatencyMode))
        {
            try
            {
                var latencyMode = Enum.Parse<GCLatencyMode>(options.LatencyMode, true);
                GCSettings.LatencyMode = latencyMode;
            }
            catch
            {
                // Mantém o modo padrão se não conseguir parsear
            }
        }
    }

    private static GCOptions LoadGCOptions(IConfiguration configuration)
    {
        var options = new GCOptions();

        var section = configuration.GetSection(CONFIG_SECTION);
        if (section.Exists())
        {
            options.IsServerGC = section.GetValue(SERVER_GC_KEY, true);
            options.LatencyMode = section.GetValue(LATENCY_MODE_KEY, "Interactive") ?? "Interactive";
        }

        return options;
    }

    public static bool IsServerGCEnabled()
    {
        return AppContext.TryGetSwitch("System.GC.Server", out var enabled) && enabled;
    }

    public static string GetCurrentGCMode()
    {
        return IsServerGCEnabled() ? "Server GC" : "Workstation GC";
    }

    public static void LogGCConfiguration(ILogger logger, GCOptions options)
    {
        logger.LogInformation(
            "🚀 GC Configuration: ServerGC={ServerGC}, LatencyMode={LatencyMode}, " +
            "Atualmente usando: {CurrentMode}",
            options.IsServerGC,
            options.LatencyMode,
            GetCurrentGCMode()
        );
    }
}
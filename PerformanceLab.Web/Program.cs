using PerformanceLab.Web.Components;
using PerformanceLab.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

/// ✅ CONFIGURAÇÃO DO GC 
var gcOptions = builder.Configuration.ConfigureGC(builder.Services);

var logger = LoggerFactory.Create(cfg => cfg.AddConsole()).CreateLogger("Program");
GCConfiguration.LogGCConfiguration(logger, gcOptions);

// Serviços
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddHostedService<MetricsCollector>();

builder.Services.AddControllers();

// HttpClient para o Dashboard
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapBlazorHub();
app.MapHub<MetricsHub>("/metricsHub");
app.MapControllers(); // Para API dos benchmarks






app.Run();

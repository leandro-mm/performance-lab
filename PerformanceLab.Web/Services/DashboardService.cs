// Services/DashboardService.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using PerformanceLab.Web.Models;
using System.Text.Json;

public class DashboardService : IDashboardService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJSRuntime _jsRuntime;

    private PerformanceMetrics? _currentMetrics;
    private List<PerformanceMetrics> _history = new();
    private bool _isConnected = false;
    private bool _isBenchmarkRunning = false;
    private bool _isCleanButtonDisabled = true;

    // Events
    public event Action<PerformanceMetrics>? OnMetricsReceived;
    public event Action<List<PerformanceMetrics>>? OnHistoryReceived;
    public event Action<bool, string>? OnConnectionStatusChanged;
    public event Action<bool>? OnBenchmarkRunningChanged;
    public event Action<BenchmarkResultDto?>? OnBenchmarkResultReceived;

    // Properties
    public PerformanceMetrics? CurrentMetrics => _currentMetrics;
    public List<PerformanceMetrics> History => _history;
    public bool IsConnected => _isConnected;
    public bool IsBenchmarkRunning => _isBenchmarkRunning;
    public bool IsCleanButtonDisabled
    {
        get => _isCleanButtonDisabled;
        set => _isCleanButtonDisabled = value;
    }

    public DashboardService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
    {
        _httpClientFactory = httpClientFactory;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync(NavigationManager navigation)
    {
        Console.WriteLine("🔵 DashboardService Initializing...");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigation.ToAbsoluteUri("/metricsHub"))
            .WithAutomaticReconnect()
            .Build();

        // Setup SignalR events
        _hubConnection.Closed += OnConnectionClosed;
        _hubConnection.Reconnected += OnConnectionReconnected;
        _hubConnection.On<PerformanceMetrics>("ReceiveMetrics", OnReceiveMetrics);
        _hubConnection.On<List<PerformanceMetrics>>("ReceiveHistory", OnReceiveHistory);

        try
        {
            await _hubConnection.StartAsync();
            _isConnected = true;
            OnConnectionStatusChanged?.Invoke(true, "Conectado ao MetricsHub");
            Console.WriteLine("✅ SignalR Conectado ao MetricsHub");
        }
        catch (Exception ex)
        {
            OnConnectionStatusChanged?.Invoke(false, $"❌ Erro: {ex.Message}");
            Console.WriteLine($"❌ SignalR connection error: {ex.Message}");
        }
    }

    private Task OnConnectionClosed(Exception? error)
    {
        _isConnected = false;
        OnConnectionStatusChanged?.Invoke(false, "❌ Desconectado do MetricsHub");
        Console.WriteLine("🔴 desconectado do MetricsHub");
        return Task.CompletedTask;
    }

    private Task OnConnectionReconnected(string? connectionId)
    {
        _isConnected = true;
        OnConnectionStatusChanged?.Invoke(true, $"Reconectado ao MetricsHub (ID: {connectionId})");
        Console.WriteLine($"🔄 reconectado ao MetricsHub: {connectionId}");
        return Task.CompletedTask;
    }

    private void OnReceiveMetrics(PerformanceMetrics metrics)
    {
        _currentMetrics = metrics;
        _history.Add(metrics);

        if (_history.Count > 100)
        {
            _history.RemoveAt(0);
        }

        OnMetricsReceived?.Invoke(metrics);
    }

    private void OnReceiveHistory(List<PerformanceMetrics> history)
    {
        _history = history;
        OnHistoryReceived?.Invoke(history);
    }

    public async Task RunBenchmarkAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var response = await httpClient.PostAsync("/api/benchmark/run", null);
            var result = await response.Content.ReadFromJsonAsync<BenchmarkResponseDto>();

            if (response.IsSuccessStatusCode)
            {
                _isBenchmarkRunning = true;
                OnBenchmarkRunningChanged?.Invoke(true);

                // Start polling
                _ = PollBenchmarkStatusAsync();
            }
            else
            {
                _isBenchmarkRunning = false;
                OnBenchmarkRunningChanged?.Invoke(false);
                throw new Exception(result?.Message ?? "Failed to start benchmark");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro no benchmark: {ex.Message}");
            _isBenchmarkRunning = false;
            OnBenchmarkRunningChanged?.Invoke(false);
            throw;
        }
    }

    public async Task CancelBenchmarkAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var response = await httpClient.PostAsync("/api/benchmark/cancel", null);

            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(2000);
                await CheckBenchmarkStatusAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao cancelar benchmark: {ex.Message}");
            throw;
        }
    }

    public async Task SimulateMemoryLeakAsync(int mb = 5)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var response = await httpClient.PostAsync($"/api/memory-leak/simulate-leak?mb={mb}", null);

            if (response.IsSuccessStatusCode)
            {
                _isCleanButtonDisabled = false;
                OnBenchmarkRunningChanged?.Invoke(false); // Reuse for UI update
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(string.IsNullOrWhiteSpace(error)
                    ? $"Status Code: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    : error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao simular memory leak: {ex.Message}");
            throw;
        }
    }

    public async Task CleanMemoryLeakAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            await httpClient.PostAsync("/api/memory-leak/clear-leak", null);
            _isCleanButtonDisabled = true;
            OnBenchmarkRunningChanged?.Invoke(false); // Reuse for UI update
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao limpar memory leak: {ex.Message}");
            throw;
        }
    }

    public async Task CheckBenchmarkStatusAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var response = await httpClient.GetFromJsonAsync<BenchmarkResponseDto>("/api/benchmark/status");

            var wasRunning = _isBenchmarkRunning;
            _isBenchmarkRunning = response?.IsRunning ?? false;

            if (wasRunning != _isBenchmarkRunning)
            {
                OnBenchmarkRunningChanged?.Invoke(_isBenchmarkRunning);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao verificar status: {ex.Message}");
        }
    }

    public async Task LoadBenchmarkResultsAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var result = await httpClient.GetFromJsonAsync<BenchmarkResultDto>("/api/benchmark/results");

            if (result != null && (result.TotalBenchmarks > 0 || result.IsCancelled == true))
            {
                OnBenchmarkResultReceived?.Invoke(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao carregar resultados: {ex.Message}");
            throw;
        }
    }

    private async Task PollBenchmarkStatusAsync()
    {
        try
        {
            while (_isBenchmarkRunning)
            {
                await Task.Delay(2000);
                await CheckBenchmarkStatusAsync();

                if (!_isBenchmarkRunning)
                {
                    await LoadBenchmarkResultsAsync();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro no polling: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
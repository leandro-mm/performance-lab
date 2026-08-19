public class MemoryLeakSimulator
{
    // simula vazamento de memória (para testar)
    private static List<byte[]> _memoryHog = new();
    public void SimulateMemoryLeak(int sizeInMB)
    {
        var data = new byte[sizeInMB * 1024 * 1024];
        _memoryHog.Add(data); // Mantém referência, impedindo GC
    }
}
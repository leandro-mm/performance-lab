namespace PerformanceLab.Core;

public class MemoryLeakSimulator
{
    #region Métodos com Baixa Performance



    // Retornar uma string
    public string ProcessLargeString_Bad(int iterations)
    {
        var list = new List<string>();
        for (int i = 0; i < iterations; i++)
        {
            // Isso aloca uma nova string a cada iteração
            list.Add($"Item {i:00000} - {Guid.NewGuid():N} - {DateTime.Now.Ticks}");
        }
        // Concatenação ineficiente (gera várias strings intermediárias)
        var result = "";
        foreach (var item in list)
        {
            result += item + Environment.NewLine;
        }
        return result;
    }

    // Boxing e alocações desnecessárias
    public decimal CalculateAverage_Bad(int[] numbers)
    {
        object sum = 0; // Boxing!
        for (int i = 0; i < numbers.Length; i++)
        {
            sum = (int)sum + numbers[i]; // Unboxing a cada iteração
        }
        return (int)sum / numbers.Length;
    }

    // simula vazamento de memória (para testar)
    private static List<byte[]> _memoryHog = new();
    public void SimulateMemoryLeak(int sizeInMB)
    {
        var data = new byte[sizeInMB * 1024 * 1024];
        _memoryHog.Add(data); // Mantém referência, impedindo GC
    }

    
    #endregion

    #region Métodos com Melhor Performance

    public string ProcessLargeString_Good(int iterations)
    {
        // StringBuilder evita alocações desnecessárias
        var sb = new StringBuilder(iterations * 50); // Capacidade inicial estimada

        for (int i = 0; i < iterations; i++)
        {
            sb.Append("Item ");
            sb.Append(i.ToString("D5"));
            sb.Append(" - ");
            sb.Append(Guid.NewGuid().ToString("N"));
            sb.Append(" - ");
            sb.Append(DateTime.Now.Ticks);
            sb.AppendLine();
        }

        return sb.ToString();
    }    

    public decimal CalculateAverage_Good(int[] numbers)
    {
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
        {
            sum += numbers[i];
        }
        return sum / numbers.Length;
    }

    
    #endregion
}

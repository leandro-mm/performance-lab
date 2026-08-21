// In your API project, create or update a controller
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/memory-leak")]
public class MemoryLeakController : ControllerBase
{
    private static readonly List<byte[]> _memoryLeakStorage = new();

    [HttpPost("simulate-leak")]
    public IActionResult SimulateMemoryLeak([FromQuery] int mb = 5)
    {
        try
        {
            // Allocate memory to simulate a leak
            var memoryToAllocate = mb * 1024 * 1024; // Convert MB to bytes
            var data = new byte[memoryToAllocate];

            // Fill with random data to prevent optimization
            new Random().NextBytes(data);

            // Store in static list to prevent garbage collection
            _memoryLeakStorage.Add(data);

            Console.WriteLine($"💣 Memory leak simulated: {mb}MB allocated. Total leaked: {_memoryLeakStorage.Sum(x => x.Length) / (1024 * 1024)}MB");

            return Ok(new
            {
                Message = $"Memory leak of {mb}MB simulated successfully",
                AllocatedMB = mb,
                TotalLeakedMB = _memoryLeakStorage.Sum(x => x.Length) / (1024 * 1024)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("clear-leak")]
    public IActionResult ClearMemoryLeak()
    {
        var totalBefore = _memoryLeakStorage.Sum(x => x.Length) / (1024 * 1024);
        _memoryLeakStorage.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        return Ok(new
        {
            Message = "Memory leak cleared",
            FreedMB = totalBefore
        });
    }
}
using System.Collections.Concurrent;
using System.Diagnostics;

internal static class Program
{
    public static async Task Main()
    {
        var numbers = Enumerable.Range(35_000, 2_000).ToArray();

        Measure("Sequential CPU", () =>
        {
            var results = numbers.Select(CpuIntensiveWork).ToList();
            Console.WriteLine($"Results: {results.Count}");
        });

        Measure("Parallel.ForEach CPU", () =>
        {
            var results = new ConcurrentBag<int>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(numbers, options, number =>
            {
                results.Add(CpuIntensiveWork(number));
            });

            Console.WriteLine($"Results: {results.Count}");
        });

        await MeasureAsync("Parallel.ForEachAsync mixed", async () =>
        {
            var results = new ConcurrentBag<int>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10
            };

            await Parallel.ForEachAsync(numbers, options, async (number, token) =>
            {
                await Task.Delay(1, token);
                results.Add(CpuIntensiveWork(number));
            });

            Console.WriteLine($"Results: {results.Count}");
        });
    }

    private static int CpuIntensiveWork(int value)
    {
        var result = 0;
        for (var i = 2; i < 250; i++)
        {
            if (value % i == 0)
            {
                result++;
            }
        }

        return result;
    }

    private static void Measure(string name, Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        Console.WriteLine($"{name}: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine();
    }

    private static async Task MeasureAsync(string name, Func<Task> action)
    {
        var stopwatch = Stopwatch.StartNew();
        await action();
        Console.WriteLine($"{name}: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine();
    }
}

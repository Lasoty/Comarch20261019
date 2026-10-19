using System.Diagnostics;
using System.Threading;

internal static class Program
{
    private const int WorkItems = 50;

    public static async Task Main()
    {
        Console.WriteLine($"Main thread: {Environment.CurrentManagedThreadId}");
        Console.WriteLine();

        RunWithThreads();
        RunWithThreadPool();
        await RunWithTasksAsync();
    }

    private static void RunWithThreads()
    {
        Console.WriteLine("=== Thread ===");
        var stopwatch = Stopwatch.StartNew();
        var threads = new List<Thread>();

        for (var i = 1; i <= WorkItems; i++)
        {
            var jobId = i;
            var thread = new Thread(() => SimulateWork(jobId, "Thread"));
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine($"Thread finished in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine();
    }

    private static void RunWithThreadPool()
    {
        Console.WriteLine("=== ThreadPool ===");
        var stopwatch = Stopwatch.StartNew();
        using var countdown = new CountdownEvent(WorkItems);

        for (var i = 1; i <= WorkItems; i++)
        {
            var jobId = i;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    SimulateWork(jobId, "ThreadPool");
                }
                finally
                {
                    countdown.Signal();
                }
            });
        }

        countdown.Wait();
        Console.WriteLine($"ThreadPool finished in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine();
    }

    private static async Task RunWithTasksAsync()
    {
        Console.WriteLine("=== Task.Run ===");
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (var i = 1; i <= WorkItems; i++)
        {
            var jobId = i;
            tasks.Add(Task.Run(() => SimulateWork(jobId, "Task.Run")));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"Task.Run finished in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine();
    }

    private static void SimulateWork(int jobId, string mechanism)
    {
        Console.WriteLine($"{mechanism,-10} job {jobId,2} started on thread {Environment.CurrentManagedThreadId}");
        Thread.Sleep(250);
        Console.WriteLine($"{mechanism,-10} job {jobId,2} ended   on thread {Environment.CurrentManagedThreadId}");
    }
}
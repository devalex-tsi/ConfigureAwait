using HtmlAgilityPack;
using System.Diagnostics;

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        int iterations = 1000;  
        int parallelTasks = 10;
        string[] urls =
        {
            "https://www.wikipedia.org",
            "https://www.github.com"
        };

        Console.WriteLine("It may take about 10 minutes for all tests");
        Console.WriteLine();

        Console.WriteLine("Measuring performance for a simple method without ConfigureAwait(false) in parallel:");
        var timeWithoutConfigureAwait = await MeasurePerformance(DownloadWithoutConfigureAwait, iterations, parallelTasks);
        Console.WriteLine($"Time for a simple method without ConfigureAwait(false) in parallel: {timeWithoutConfigureAwait} ms");

        Console.WriteLine("Measuring performance for a simple method with ConfigureAwait(false) in parallel:");
        var timeWithConfigureAwait = await MeasurePerformance(DownloadWithConfigureAwait, iterations, parallelTasks);
        Console.WriteLine($"Time for a simple method with ConfigureAwait(false) in parallel: {timeWithConfigureAwait} ms");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Performance difference:");
        Console.WriteLine($"Without ConfigureAwait(false) in parallel: {timeWithoutConfigureAwait} ms");
        Console.WriteLine($"With ConfigureAwait(false) in parallel: {timeWithConfigureAwait} ms");
        Console.ResetColor();

        Console.WriteLine("Measuring performance with complex method without ConfigureAwait(false) in parallel:");
        timeWithoutConfigureAwait = await MeasurePerformance(() => DownloadAndParseWithoutConfigureAwait(urls), iterations, parallelTasks);
        Console.WriteLine($"Time without ConfigureAwait(false) in parallel: {timeWithoutConfigureAwait} ms");

        Console.WriteLine("Measuring performance with complex method with ConfigureAwait(false) in parallel:");
        timeWithConfigureAwait = await MeasurePerformance(() => DownloadAndParseWithConfigureAwait(urls), iterations, parallelTasks);
        Console.WriteLine($"Time with ConfigureAwait(false) in parallel: {timeWithConfigureAwait} ms");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Performance difference:");
        Console.WriteLine($"Without ConfigureAwait(false) in parallel: {timeWithoutConfigureAwait} ms");
        Console.WriteLine($"With ConfigureAwait(false) in parallel: {timeWithConfigureAwait} ms");
        Console.ResetColor();
    }

    // Simple method asynchronous method without using ConfigureAwait(false)
    static async Task DownloadWithoutConfigureAwait()
    {
        // Simulation of an asynchronous request
        var content = await httpClient.GetStringAsync("https://example.com");
        // Executing other code after await
        // await Task.Delay(10);
    }

    // Simple method asynchronous method with using ConfigureAwait(false)
    static async Task DownloadWithConfigureAwait()
    {
        // Asynchronous request with ConfigureAwait(false)
        var content = await httpClient.GetStringAsync("https://example.com").ConfigureAwait(false);
        // Executing other code after await
        // await Task.Delay(10).ConfigureAwait(false);
    }

    // Complex method asynchronous method without using ConfigureAwait(false)
    static async Task DownloadAndParseWithoutConfigureAwait(string[] urls)
    {
        foreach (var url in urls)
        {
            var content = await httpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(content);
            
            var titleNode = document.DocumentNode.SelectSingleNode("//title");
            var title = titleNode?.InnerText;
            await Task.Delay(10);
        }
    }

    // Complex method asynchronous method with using ConfigureAwait(false)
    static async Task DownloadAndParseWithConfigureAwait(string[] urls)
    {
        foreach (var url in urls)
        {
            var content = await httpClient.GetStringAsync(url).ConfigureAwait(false);
            var document = new HtmlDocument();
            document.LoadHtml(content);
            
            var titleNode = document.DocumentNode.SelectSingleNode("//title");
            var title = titleNode?.InnerText;
            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    // Measuring performance with parallel execution
    static async Task<long> MeasurePerformance(Func<Task> asyncMethod, int iterations, int parallelTasks)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // Running parallel tasks
        for (int i = 0; i < iterations / parallelTasks; i++)
        {
            var tasks = new Task[parallelTasks];
            for (int j = 0; j < parallelTasks; j++)
            {
                tasks[j] = asyncMethod();
            }
            await Task.WhenAll(tasks);
        }

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}

using PrWatch.Azure;

namespace PrWatch;

public static class Program
{
  public static async Task Main(string[] args)
  {
    DevOpsConfig config = new DevOpsConfigFactory().Get();

    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, e) =>
    {
      Console.Out.WriteLine("Exiting");
      cts.Cancel();
    };
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    IPrService service = new PrService(config);
    IPrWatcher watcher = new PrWatcher(service, config.PrUrl); 
    watcher.StatusChanged.Subscribe(async statuses =>
    {
      Console.Clear();

      foreach (string status in statuses)
      {
        await Console.Out.WriteLineAsync(status);
      }
    });

    await service.RunAsync(cts.Token);
  }
}

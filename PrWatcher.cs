using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace PrWatch;

public interface IPrWatcher
{
  IObservable<List<string>> StatusChanged { get; }
}

public class PrWatcher : IPrWatcher
{
  private readonly ConcurrentDictionary<int, PrStatus> cache_ = new();
  private readonly SemaphoreSlim sem_ = new(1, 1);
  private readonly IPrService service_;

  public string PrUrl { get; set; } 
  public IObservable<List<string>> StatusChanged => statusChanged_;
  private readonly Subject<List<string>> statusChanged_ = new();

  public PrWatcher(IPrService service, string prUrl)
  {
    service_ = service;
    PrUrl = prUrl;
    service_.StatusChanged.Subscribe(async pr => await HandleStatusChanged(pr));
  }

  private async Task HandleStatusChanged(PrStatus pr)
  {
    cache_[pr.Id] = pr;

    List<string> statuses = cache_.Values
      .OrderBy(pr => pr.Id)
      .Select(pr => pr.ToNiceString(PrUrl))
      .ToList();

    await sem_.WaitAsync();
    statusChanged_.OnNext(statuses);
    sem_.Release();
  }
}
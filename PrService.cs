using PrWatch.Azure;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Text.Json;

namespace PrWatch;

public interface IPrService
{
  TimeSpan PollInterval { get; }
  IObservable<PrStatus> StatusChanged { get; }

  Task RunAsync(CancellationToken ct);
}

public class PrService : IPrService
{
  public IObservable<PrStatus> StatusChanged => statusChanged_;
  private readonly Subject<PrStatus> statusChanged_ = new();

  public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

  private readonly JsonSerializerOptions options_ = new()
  {
    PropertyNameCaseInsensitive = true,
  };

  private readonly ConcurrentDictionary<Process, Process> procs_ = new();
  private readonly DevOpsConfig info_;

  public PrService(DevOpsConfig info)
  {
    info_ = info;
  }

  public async Task RunAsync(CancellationToken ct)
  {
    while (!ct.IsCancellationRequested)
    {
      try
      {
        await RunOnceAsync(ct);
        await Task.Delay(PollInterval, ct);
      }
      catch (TaskCanceledException)
      {
        // expected
      }
      catch (Exception e)
      {
        await Console.Error.WriteLineAsync($"{e}");
      }
    }

    foreach (var proc in procs_.Values)
    {
      proc.Kill();
    }
  }

  private async Task RunOnceAsync(CancellationToken ct)
  {
    List<PullRequest>? prs = await GetPullRequestsAsync(ct);

    if (prs == null)
    {
      return;
    }

    foreach (PullRequest pr in prs)
    {
      _ = Task.Run(async () =>
      {
        List<Azure.Build> builds = await GetBuildsAsync(pr, ct);
        if (!builds.Any())
        {
          return;
        }

        PrStatus status = GetOutput(pr, builds);
        statusChanged_.OnNext(status);
      }, ct);
    }
  }

  private static PrStatus GetOutput(PullRequest pr, List<Azure.Build> builds)
  {
    var prStatus = new PrStatus()
    {
      Title = pr.Title,
      Id = pr.Id,
    };

    foreach (Azure.Build build in builds)
    {
      BuildStatus status = build.Status switch
      {
        "completed" when build.Result == "succeeded" => BuildStatus.Success,
        "completed" when build.Result == "failed" => BuildStatus.Failed,
        "inProgress" => BuildStatus.InProgress,
        _ => BuildStatus.Unknown,
      };

      prStatus.Builds.Add(new Build
      {
        Id = build.Id,
        Name = build.Definition?.Name!,
        Status = status
      });
    }

    return prStatus;
  }

  private async Task<List<PullRequest>?> GetPullRequestsAsync(CancellationToken ct)
  {
    string constraints = $"--query \"[? contains(not_null(status,''),'active') && !isDraft]\" --creator {info_.PrCreator} " +
      $"--repository {info_.RepositoryName} --org {info_.OrganizationUrl} --project {info_.ProjectName} --output json --only-show-errors";

    string command = $"az repos pr list {constraints}";
    string output = await RunCommandAsync(command, ct);

    return JsonSerializer.Deserialize<List<PullRequest>>(output, options_);
  }

  private async Task<List<Azure.Build>> GetBuildsAsync(PullRequest pr, CancellationToken ct)
  {
    string constraints = $"--org {info_.OrganizationUrl} --project {info_.ProjectName} --output json --only-show-errors";

    string command = $"az pipelines runs list {constraints} --branch refs/pull/{pr.Id}/merge";
    string output = await RunCommandAsync(command, ct);

    var builds = JsonSerializer.Deserialize<List<Azure.Build>>(output, options_);

    return builds == null
      ? new List<Azure.Build>()
      : builds.Where(build => build.SourceVersion == pr.LastMergeCommit?.Id).ToList();
  }

  private async Task<string> RunCommandAsync(string command, CancellationToken ct)
  {
    var startInfo = new ProcessStartInfo
    {
      FileName = "cmd.exe",
      Arguments = $"/c {command}",
      RedirectStandardOutput = true,
      RedirectStandardInput = false,
      UseShellExecute = false
    };
    startInfo.EnvironmentVariables["AZURE_DEVOPS_EXT_PAT"] = info_.PersonalAccessToken;

    var process = new Process
    {
      StartInfo = startInfo
    };

    procs_[process] = process;
    process.Start();

    string output = await process.StandardOutput.ReadToEndAsync();

    await process.WaitForExitAsync(ct);
    procs_.TryRemove(process, out _);

    return output;
  }
}

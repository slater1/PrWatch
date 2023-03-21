namespace PrWatch.Azure;

/// <summary>
/// Represents a build in Azure Pipelines.
/// </summary>
public class Build
{
  public int Id { get; set; }
  public BuildDefinition? Definition { get; set; }
  public DateTime? QueueTime { get; set; }
  public DateTime? StartTime { get; set; }
  public DateTime? FinishTime { get; set; }
  public string? Status { get; set; }
  public string? Result { get; set; }
  /// <summary>
  /// SHA1 commit ID
  /// </summary>
  public string? SourceVersion { get; set; }
}

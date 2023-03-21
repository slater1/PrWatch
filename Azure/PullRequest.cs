using System.Text.Json.Serialization;

namespace PrWatch.Azure;

/// <summary>
/// Represents a pull request in a Git repository.
/// </summary>
public class PullRequest
{
  [JsonPropertyName("PullRequestId")]
  public int Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public DateTime CreationDate { get; set; }
  /// <summary>
  /// SHA1 commit ID
  /// </summary>
  public Commit? LastMergeCommit { get; set; }
}
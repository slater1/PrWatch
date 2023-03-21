using System.Text.Json.Serialization;
namespace PrWatch.Azure;

public class Commit
{
  [JsonPropertyName("CommitId")]
  public string? Id { get; set; }
}

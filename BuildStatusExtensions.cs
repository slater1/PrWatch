namespace PrWatch;

public static class BuildStatusExtensions
{ 
  public static string MapIcon(this Build build)
  {
    return build.Status switch
    {
      BuildStatus.Success => "✔",
      BuildStatus.Failed => "✘",
      BuildStatus.InProgress => "⟳",
      _ => "?"
    };
  }
}

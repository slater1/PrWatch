namespace PrWatch;

public static class PrStatusExtensions
{
  public static string ToNiceString(this PrStatus pr, string url)
  {
    string output = $"Pull Request #{pr.Id} - {pr.Title}"
      + $"\n  {string.Format(url, pr.Id)}\n";

    if (!pr.Builds.Any())
    {
      output += "  No builds found";
    }

    foreach (Build build in pr.Builds)
    {
      string icon = build.MapIcon();

      output += $"  {icon} {build.Name ?? $"{build.Id}"}\n";
    }

    return output;
  }
}
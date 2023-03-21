namespace PrWatch.Azure;

public class DevOpsConfig
{
  public string? OrganizationUrl { get; set; }
  public string PrUrl { get; set; } = "https://tfs.company.com/tfs/SomeCollection/SomeProject/_git/SomeRepo/pullrequest/{0}"; 
  public string? PersonalAccessToken { get; set; }
  public string? ProjectName { get; set; }
  public string? RepositoryName { get; set; }
  public string? PrCreator { get; set; }
}

using Microsoft.Extensions.Configuration;
using PrWatch.Azure;
using System.Reflection;

namespace PrWatch;

public class DevOpsConfigFactory
{
  public DevOpsConfig Get()
  {
    string? cwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    var settings = new ConfigurationBuilder()
      .SetBasePath(cwd ?? Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json")
      .AddUserSecrets<DevOpsConfig>()
      .Build()
      .GetSection("Azure");

    var config = new DevOpsConfig
    {
      OrganizationUrl = settings.GetSection(nameof(DevOpsConfig.OrganizationUrl)).Value,
      PrUrl = settings.GetSection(nameof(DevOpsConfig.PrUrl)).Value,
      PersonalAccessToken = settings.GetSection(nameof(DevOpsConfig.PersonalAccessToken)).Value,
      ProjectName = settings.GetSection(nameof(DevOpsConfig.ProjectName)).Value,
      RepositoryName = settings.GetSection(nameof(DevOpsConfig.RepositoryName)).Value,
      PrCreator = settings.GetSection(nameof(DevOpsConfig.PrCreator)).Value,
    };
    return config;
  }
}
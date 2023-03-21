namespace PrWatch;

public class PrStatus
{
  public string Title { get; set; } = "";
  public int Id { get; set; }
  public List<Build> Builds { get; set; } = new();
}

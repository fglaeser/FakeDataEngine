namespace FakeDataEngine
{
  public class Config
  {
    public string ConnectionString { get; set; }

    public int ThrottleMs { get; set; } = 1000;
  }
}

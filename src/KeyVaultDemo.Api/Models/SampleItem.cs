namespace KeyVaultDemo.Api.Models;

public record SampleItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

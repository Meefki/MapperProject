namespace MapperProject.Tests.Setup.Models;

public class Dto
{
    public int? Id { get; set; } = null!;
    public List<string> Strings { get; set; } = new List<string>();
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Number { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
}

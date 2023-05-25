namespace MapperProject.Tests.Setup.Models;

public class DomainModel
{
    private List<string> _numbers;
    public IReadOnlyCollection<string> Numbers => _numbers.AsReadOnly();

    public int Id { get; private set; }
    //public PersonInfo PersonInfo { get; private set; }

    public string Ignore { get; } = "Field for ignoring";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private DomainModel() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public DomainModel(
        int id,
        //PersonInfo personInfo,
        IEnumerable<string> numbers)
    {
        Id = id;
        //PersonInfo = personInfo;
        _numbers = numbers.ToList();
    }
}

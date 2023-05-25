using MapperProject.Tests.Setup.Models;

namespace MapperProject.Tests.Setup;

public static class MockData
{
    public static DomainModel CreateDestMock()
    {
        PersonInfo personInfo = new("John", "Сena", "8-800-555-35-35", 34, "Male");

        DomainModel domainModel = new(1, personInfo, new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" });

        return domainModel;
    }

    public static Dto CreateSourceMock()
        => new()
        { 
            Id = 1, 
            Age = 34,
            FirstName = "John",
            LastName = "Cena",
            Gender = "Male",
            Number = "8-800-555-35-35",
            Strings = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" } 
        };
}

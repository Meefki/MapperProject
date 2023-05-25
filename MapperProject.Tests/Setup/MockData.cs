using MapperProject.Tests.Setup.Models;

namespace MapperProject.Tests.Setup;

public static class MockData
{
    public static DomainModel CreateDestMock()
        => new(1, new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" });

    public static Dto CreateSourceMock()
        => new() { Id = 1, Strings = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" } };
}

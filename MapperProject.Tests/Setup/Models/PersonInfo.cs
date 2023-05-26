namespace MapperProject.Tests.Setup.Models
{
    public record PersonInfo(string FirstName, string LastName, string PhoneNumber, int Age, string Gender)
    {
        private PersonInfo()
            : this(null!, null!, null!, 0, null!)
        { }
    }
}
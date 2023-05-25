using MapperProject.Models;
using MapperProject.Tests.Setup.Models;

namespace MapperProject.Tests.Setup;

public class DomainToDtoConfiguration
    : Configuration<Dto, DomainModel>
{
    public DomainToDtoConfiguration()
    {
        this.Property(dest => dest.Strings)
            .MapFrom(src => src.Numbers);
    }
}

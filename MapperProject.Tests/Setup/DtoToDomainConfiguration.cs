using MapperProject.Models;
using MapperProject.Tests.Setup.Models;

namespace MapperProject.Tests.Setup;

public class DtoToDomainConfiguration
    : Configuration<DomainModel, Dto>
{
    public override void Configure(Configuration<DomainModel, Dto> config)
    {
        config.Property(dest => dest.Numbers)
            .HasField("_numbers")
            .MapFrom(src => src.Strings);

        config.Property(dest => dest.Ignore)
            .Ignore();

        //config.NestedProperty(dest => dest.PersonInfo).MapFrom(src => src)
        //    .Property(dest => dest.PhoneNumber).MapFrom(src => src.Number);
    }
}

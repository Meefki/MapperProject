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

        config.NestedProperty(dest => dest.PersonInfo, source => source, config =>
        {
            config.Property(dest => dest.PhoneNumber)
                .MapFrom(source => source.Number);
        });
    }
}

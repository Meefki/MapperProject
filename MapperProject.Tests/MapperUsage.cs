using MapperProject.Abstractions;
using MapperProject.Tests.Setup;
using MapperProject.Tests.Setup.Models;

namespace MapperProject.Tests
{
    public class MapperUsage
    {
        [Fact]
        public void MapDtoToDomain_Success()
        {
            // Arrange
            IMapper mapper = new Mapper();
            mapper.AddConfiguration<DomainModel, Dto>(config =>
            {
                config.Property(dest => dest.Numbers)
                .HasField("_numbers")
                .MapFrom(src => src.Strings);

                config.Property(dest => dest.Ignore)
                    .Ignore();

                config.NestedProperty(dest => dest.PersonInfo, src => src, config =>
                {
                    config.Property(dest => dest.PhoneNumber)
                        .MapFrom(src => src.Number);
                });
            });

            //mapper.AddConfiguration<PersonInfo, Dto>(config =>
            //{
            //    config
            //        .Property(dest => dest.PhoneNumber)
            //        .MapFrom(src => src.Number);
            //});

            Dto source = MockData.CreateSourceMock();

            // Act
            DomainModel dest = mapper.Map<DomainModel, Dto>(source);

            // Assert
            Assert.Equal(source.Id, dest.Id);
            Assert.Equal(source.Strings, dest.Numbers);
            Assert.Null(dest.Ignore);
            Assert.Equal(source.FirstName, dest.PersonInfo.FirstName);
            Assert.Equal(source.Number, dest.PersonInfo.PhoneNumber);
            Assert.Equal(source.Age, dest.PersonInfo.Age);
        }
    }
}
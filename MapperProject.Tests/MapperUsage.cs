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
            mapper.AddConfiguration(new DtoToDomainConfiguration());

            Dto source = MockData.CreateSourceMock();

            // Act
            DomainModel dest = mapper.Map<DomainModel, Dto>(source);

            // Assert
            Assert.Equal(source.Id, dest.Id);
            Assert.Equal(source.Strings, dest.Numbers);
        }
    }
}
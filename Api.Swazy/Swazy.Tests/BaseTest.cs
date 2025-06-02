using AutoFixture;
using AutoMapper;
using Api.Swazy.Profiles;

namespace Swazy.Tests;

public class BaseTest
{
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;

    public BaseTest()
    {
        Fixture = new Fixture();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        Mapper = config.CreateMapper();
    }
}

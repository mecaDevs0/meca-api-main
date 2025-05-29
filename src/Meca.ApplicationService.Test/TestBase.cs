using System.IO;
using AutoMapper;
using Bogus;
using Meca.Data.Enum;
using Meca.Domain.AutoMapper;
using Meca.Shared.ObjectValues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using UtilityFramework.Application.Core3;

namespace Meca.ApplicationService.Test
{
    public abstract class TestBase
    {
        protected IHostingEnvironment _mockEnvironment;
        public IMapper _mockMapper;
        protected IConfiguration _mockConfiguration;
        protected Acesso _mockAccess;
        public readonly Faker _faker = new("pt_BR");

        public TestBase()
        {
            InicializaMock();
        }

        public void InicializaMock(Acesso acesso = null, string environment = "Development")
        {

            var mockEnvironment = new Mock<IHostingEnvironment>();

            _mockAccess = acesso ?? new Acesso("", (int)TypeProfile.Profile);

            mockEnvironment
                 .Setup(m => m.EnvironmentName)
                 .Returns(environment);

            _mockEnvironment = mockEnvironment.Object;

            var conf = new MapperConfiguration(cf =>
            {
                cf.AddProfile(new DomainToViewModelMappingProfile());
                cf.AddProfile(new ViewModelToDomainMappingProfile());
            });

            _mockMapper = conf.CreateMapper(); //MockaModel<TSource, TDestination>(model);

            var path = Directory.GetCurrentDirectory();

            _mockConfiguration = new ConfigurationBuilder()
                   .AddJsonFile($"appsettings.{environment}.json", optional: true)
                   .AddEnvironmentVariables()
                   .Build();

            var baseConfig = new BaseConfig();

            _mockConfiguration.GetSection("JWT").Bind(baseConfig);
            _mockConfiguration.GetSection("Config").Bind(baseConfig);

        }

    }
}

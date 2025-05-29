using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.ApplicationService.Services
{
    public class UserAdministratorService : ApplicationServiceBase<UserAdministrator>, IUserAdministratorService
    {
        private readonly IBusinessBaseAsync<UserAdministrator> _userAdministratorRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        private readonly IMapper _mapper;


        /*Construtor utilizado por testes de unidade*/
        public UserAdministratorService(IHostingEnvironment env, IMapper mapper, IConfiguration configuration, Acesso acesso, string testUnit)
        {
            _userAdministratorRepository = new BusinessBaseAsync<UserAdministrator>(env);

            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
        }

        public UserAdministratorService(IMapper mapper,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository,
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _mapper = mapper;
            _userAdministratorRepository = userAdministratorRepository;

            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
    }
}
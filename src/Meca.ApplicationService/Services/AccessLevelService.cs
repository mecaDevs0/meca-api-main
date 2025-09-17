using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.ApplicationService.Services
{
    public class AccessLevelService : ApplicationServiceBase<AccessLevel>, IAccessLevelService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<AccessLevel> _accessLevelRepository;
        private readonly IBusinessBaseAsync<UserAdministrator> _userAdministratorRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public AccessLevelService(
            IMapper mapper,
            IBusinessBaseAsync<AccessLevel> accessLevelRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository)
        {
            _mapper = mapper;
            _accessLevelRepository = accessLevelRepository;

            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _userAdministratorRepository = userAdministratorRepository;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public AccessLevelService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            IBusinessBaseAsync<AccessLevel> accessLevelRepository,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository,
            string testUnit)
        {

            _accessLevelRepository = accessLevelRepository;
            _userAdministratorRepository = userAdministratorRepository;
            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
        }

        public async Task<List<AccessLevelViewModel>> GetAll()
        {
            var listaccessLevel = await _accessLevelRepository.FindAllAsync(Util.Sort<AccessLevel>().Descending(nameof(AccessLevel.Created)));

            return _mapper.Map<List<AccessLevelViewModel>>(listaccessLevel);
        }

        public async Task<List<T>> GetAll<T>(AccessLevelFilterViewModel filterView) where T : class
        {

            filterView.SetDefault();

            var builder = Builders<AccessLevel>.Filter;
            var conditions = new List<FilterDefinition<AccessLevel>>();

            if (filterView.DataBlocked != null)
            {
                switch (filterView.DataBlocked.GetValueOrDefault())
                {
                    case FilterActived.Actived:
                        conditions.Add(builder.Eq(x => x.DataBlocked, null));
                        break;
                    case FilterActived.Disabled:
                        conditions.Add(builder.Ne(x => x.DataBlocked, null));
                        break;
                }
            }

            if (string.IsNullOrEmpty(filterView.Name) == false)
                conditions.Add(builder.Regex(x => x.Name, new BsonRegularExpression(filterView.Name, "i")));

            if (filterView.IsDefault == true)
                conditions.Add(builder.Eq(x => x.IsDefault, filterView.IsDefault));

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listaccessLevel = await _accessLevelRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions(filterView, Util.Sort<AccessLevel>().Ascending(x => x.Created)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listaccessLevel);
        }

        public async Task<AccessLevelViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var accessLevelEntity = await _accessLevelRepository.FindByIdAsync(id);

                if (accessLevelEntity == null)
                {
                    CreateNotification(DefaultMessages.AccessLevelNotFound);
                    return null;
                }
                return _mapper.Map<AccessLevelViewModel>(accessLevelEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<AccessLevelViewModel> Register(AccessLevelViewModel model)
        {

            try
            {
                if (ModelIsValid(model, true) == false)
                    return null;

                var accessLevelEntity = _mapper.Map<AccessLevel>(model);

                accessLevelEntity = await _accessLevelRepository.CreateReturnAsync(accessLevelEntity);

                return _mapper.Map<AccessLevelViewModel>(accessLevelEntity);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<AccessLevelViewModel> UpdatePatch(string id, AccessLevelViewModel model)
        {
            try
            {
                if (ModelIsValid(model, true) == false)
                    return null;

                var accessLevelEntity = await _accessLevelRepository.FindByIdAsync(id);

                if (accessLevelEntity == null)
                {
                    CreateNotification(DefaultMessages.AccessLevelNotFound);
                    return null;
                }

                if (accessLevelEntity.IsDefault)
                {
                    CreateNotification(DefaultMessages.InvalidDeleteDefault);
                    return null;
                }

                accessLevelEntity.SetIfDifferent(model, _jsonBodyFields);

                if (_jsonBodyFields.Count(x => x.ContainsIgnoreCase(nameof(model.Rules))) > 0)
                {
                    accessLevelEntity.Rules = _mapper.Map<List<ItemMenuRule>>(model.Rules);
                }

                accessLevelEntity = await _accessLevelRepository.UpdateAsync(accessLevelEntity);

                return _mapper.Map<AccessLevelViewModel>(accessLevelEntity);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<AccessLevelViewModel> RegisterDefault()
        {
            try
            {
                var defaultAccessLevelEntity = await _accessLevelRepository.FindOneByAsync(x => x.IsDefault);

                if (defaultAccessLevelEntity == null)
                {
                    defaultAccessLevelEntity = new AccessLevel()
                    {
                        IsDefault = true,
                        Name = "Master"
                    };

                    var enumItens = Enum.GetValues<MenuItem>().ToList();
                    var enumSubItens = Enum.GetValues<SubMenuItem>().ToList();

                    for (int i = 0; i < enumItens.Count; i++)
                    {
                        var enumValue = ((int)enumItens[i]) * 100;
                        var startSubLevel = ((int)enumItens[i]) * 100;
                        var endSubLevel = startSubLevel + 99;

                        var currentSubItens = enumSubItens.Where(x => (int)x >= startSubLevel && (int)x < endSubLevel).ToList();

                        defaultAccessLevelEntity.Rules.Add(new ItemMenuRule()
                        {
                            MenuItem = enumItens[i],
                        });

                        for (int a = 0; a < currentSubItens.Count; a++)
                        {
                            defaultAccessLevelEntity.Rules.Add(new ItemMenuRule()
                            {
                                MenuItem = enumItens[i],
                                SubMenu = currentSubItens[a]
                            });
                        }
                    }

                    await _accessLevelRepository.CreateAsync(defaultAccessLevelEntity);

                }

                return _mapper.Map<AccessLevelViewModel>(defaultAccessLevelEntity);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                if (_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.OnlyAdministrator);
                    return false;
                }
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return false;
                }

                var accessLevelEntity = await _accessLevelRepository.FindByIdAsync(id);

                if (accessLevelEntity.IsDefault)
                {
                    CreateNotification(DefaultMessages.InvalidDeleteDefault);
                    return false;
                }

                await _accessLevelRepository.DeleteOneAsync(id);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RefreshMenu()
        {
            try
            {
                var enumValues = Enum.GetValues<MenuItem>();

                var totalMenu = enumValues.Length;

                var builder = Builders<AccessLevel>.Filter;
                var conditions = new List<FilterDefinition<AccessLevel>>
                {
                    builder.SizeLte(x => x.Rules, totalMenu)
                };

                var list = await _accessLevelRepository
                .GetCollectionAsync()
                .FindSync(builder.And(conditions))
                .ToListAsync();

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var hasUpdate = false;

                    var value = item.IsDefault;
                    if (item.Rules.Count < totalMenu)
                    {
                        for (int a = 0; a < enumValues.Length; a++)
                        {
                            var menuItem = enumValues[a];
                            if (item.Rules.Count(x => x.MenuItem == menuItem) == 0)
                            {
                                item.Rules.Add(new ItemMenuRule()
                                {
                                    Write = value,
                                    Access = value,
                                    Delete = value,
                                    Edit = value,
                                    EnableDisable = value,
                                    Export = value,
                                    MenuItem = menuItem,
                                });
                                hasUpdate = true;
                            }
                        }
                    }
                    if (hasUpdate)
                        await _accessLevelRepository.UpdateAsync(item, false);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
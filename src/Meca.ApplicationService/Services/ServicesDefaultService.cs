using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class ServicesDefaultService : ApplicationServiceBase<Data.Entities.ServicesDefault>, IServicesDefaultService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<Data.Entities.ServicesDefault> _servicesDefaultRepository;
        private readonly IBusinessBaseAsync<WorkshopServices> _workshopServicesRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public ServicesDefaultService(
            IMapper mapper,
            IBusinessBaseAsync<Data.Entities.ServicesDefault> servicesDefaultRepository,
            IBusinessBaseAsync<WorkshopServices> workshopServicesRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _servicesDefaultRepository = servicesDefaultRepository;
            _workshopServicesRepository = workshopServicesRepository;
            _workshopRepository = workshopRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            
            // Inicializar o acesso
            SetAccess(httpContextAccessor);
        }

        public async Task<List<ServicesDefaultViewModel>> GetAll()
        {
            var listServices = await _servicesDefaultRepository.FindAllAsync(Util.Sort<Data.Entities.ServicesDefault>().Ascending(nameof(Data.Entities.ServicesDefault.Name)));

            return _mapper.Map<List<ServicesDefaultViewModel>>(listServices);
        }

        public async Task<List<T>> GetAll<T>(ServicesDefaultFilterViewModel filterView) where T : class
        {
            try
            {
                filterView.SetDefault();

                var builder = Builders<Data.Entities.ServicesDefault>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.ServicesDefault>>();

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

                if (filterView.ServiceTypes != null && filterView.ServiceTypes.Any())
                {
                    try
                    {
                        var objectIds = filterView.ServiceTypes
                            .Where(id => ObjectId.TryParse(id, out _))
                            .Select(ObjectId.Parse)
                            .ToList();
                        
                        if (objectIds.Any())
                        {
                            conditions.Add(builder.In(x => x._id, objectIds));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVICES_DEFAULT_DEBUG] Erro ao processar ServiceTypes: {ex.Message}");
                        // Continuar sem o filtro de ServiceTypes
                    }
                }

                // ProfileId é opcional - não adicionar condição se for null ou vazio
                if (!string.IsNullOrEmpty(filterView.ProfileId))
                {
                    // Aqui você pode adicionar lógica específica para profileId se necessário
                    // Por enquanto, não fazemos nada com o profileId
                }

                var filter = conditions.Any() ? builder.And(conditions) : builder.Empty;

                var listServices = await _servicesDefaultRepository.GetCollectionAsync().Find(filter).ToListAsync();

                return _mapper.Map<List<T>>(listServices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVICES_DEFAULT_DEBUG] Erro em GetAll: {ex.Message}");
                
                // Tratamento específico para erro oldValue do MongoDB
                if (ex.Message.Contains("oldValue") || ex.InnerException?.Message?.Contains("oldValue") == true)
                {
                    Console.WriteLine($"[SERVICES_DEFAULT_DEBUG] Erro oldValue detectado, tentando abordagem alternativa: {ex.Message}");
                    
                    // Tentar abordagem alternativa sem filtros complexos
                    try
                    {
                        var listServices = await _servicesDefaultRepository.FindAllAsync();
                        return _mapper.Map<List<T>>(listServices);
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"[SERVICES_DEFAULT_DEBUG] Erro na abordagem alternativa: {fallbackEx.Message}");
                        throw;
                    }
                }
                
                throw;
            }
        }

        public async Task<ServicesDefaultViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var servicesDefaultEntity = await _servicesDefaultRepository.FindByIdAsync(id);

                if (servicesDefaultEntity == null)
                {
                    CreateNotification(DefaultMessages.ServicesNotFound);
                    return null;
                }
                return _mapper.Map<ServicesDefaultViewModel>(servicesDefaultEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ServicesDefaultViewModel> Register(ServicesDefaultViewModel model)
        {
            try
            {
                if (_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.OnlyAdministrator);
                    return null;
                }

                if (ModelIsValid(model, false) == false)
                    return null;

                var serviceExist = await _servicesDefaultRepository.FindOneByAsync(x => x.Name == model.Name);
                if (serviceExist != null)
                {
                    CreateNotification(DefaultMessages.ServiceInUse);
                    return null;
                }

                var servicesDefaultEntity = _mapper.Map<Data.Entities.ServicesDefault>(model);

                servicesDefaultEntity = await _servicesDefaultRepository.CreateReturnAsync(servicesDefaultEntity);

                return _mapper.Map<ServicesDefaultViewModel>(servicesDefaultEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ServicesDefaultViewModel> UpdatePatch(string id, ServicesDefaultViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                if (_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.OnlyAdministrator);
                    return null;
                }

                var servicesDefaultEntity = await _servicesDefaultRepository.FindByIdAsync(id);
                if (servicesDefaultEntity == null)
                {
                    CreateNotification(DefaultMessages.ServicesNotFound);
                    return null;
                }

                var _id = ObjectId.Parse(id);

                var serviceExist = await _servicesDefaultRepository.CheckByAsync(x => x.Name == model.Name && x._id != _id);
                if (serviceExist == true)
                {
                    CreateNotification(DefaultMessages.ServiceInUse);
                    return null;
                }

                servicesDefaultEntity.SetIfDifferent(model, validOnly, _mapper);

                servicesDefaultEntity = await _servicesDefaultRepository.UpdateAsync(servicesDefaultEntity);

                return _mapper.Map<ServicesDefaultViewModel>(servicesDefaultEntity);
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

                var servicesDefaultEntity = await _servicesDefaultRepository.FindByIdAsync(id);

                await _servicesDefaultRepository.DeleteOneAsync(id);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
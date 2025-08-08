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

            if (filterView.ServiceTypes != null)
            {
                conditions.Add(builder.In(x => x._id, filterView.ServiceTypes.Select(ObjectId.Parse).ToList()));
            }

            // Task para remover preço do serviço
            // if (filterView.PriceRangeInitial > 0 || filterView.PriceRangeFinal > 0)
            // {
            //     var priceMin = filterView.PriceRangeInitial > 0 ? filterView.PriceRangeInitial : 0;
            //     var priceMax = filterView.PriceRangeFinal > 0 ? filterView.PriceRangeFinal : double.MaxValue;

            //     var workshopServicesList = await _workshopServicesRepository
            //         .FindByAsync(x => x.Value >= priceMin && x.Value <= priceMax);

            //     var serviceNames = workshopServicesList.Select(x => x.Name).ToList();

            //     if (serviceNames.Any())
            //     {
            //         conditions.Add(builder.In(x => x.Name, serviceNames));
            //     }
            // }

            if (filterView.Rating != null)
            {
                var workshopList = await _workshopRepository.FindByAsync(x => x.Rating == filterView.Rating);
                var workshopCompanyNames = workshopList.Select(w => w.CompanyName).ToList();
                var workshopServicesList = await _workshopServicesRepository.FindByAsync(x => workshopCompanyNames.Contains(x.Workshop.CompanyName));
                var servicesId = workshopServicesList.Select(x => x._id).ToList();
                conditions.Add(builder.In(x => x._id, servicesId));
            }

            if (!conditions.Any())
            {
                conditions.Add(builder.Empty);
            }

            if (string.IsNullOrEmpty(filterView.LatUser) == false && string.IsNullOrEmpty(filterView.LongUser) == false)
            {
                var userLatitude = double.Parse(filterView.LatUser, CultureInfo.InvariantCulture);
                var userLongitude = double.Parse(filterView.LongUser, CultureInfo.InvariantCulture);
                var maxDistance = filterView.Distance;

                var workshopList = (List<Workshop>)await _workshopRepository.FindAllAsync();

                for (var i = 0; i < workshopList.Count; i++)
                {
                    workshopList[i].Distance = await Util.GetDistanceAsync(userLatitude, userLongitude, workshopList[i].Latitude, workshopList[i].Longitude);
                }

                var responseWithDistance = new List<Workshop>();
                if (maxDistance != null)
                {
                    responseWithDistance = workshopList.Where(Workshop => { return Workshop.Distance <= maxDistance; }).ToList();
                }
                else
                {
                    responseWithDistance = workshopList.ToList();
                }

                var workshopCompanyNames = responseWithDistance.Select(w => w.CompanyName).ToList();
                var workshopServicesList = await _workshopServicesRepository.FindByAsync(x => workshopCompanyNames.Contains(x.Workshop.CompanyName));
                var servicesId = workshopServicesList.Select(x => x._id).ToList();
                conditions.Add(builder.In(x => x._id, servicesId));
            }

            var listServices = await _servicesDefaultRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions(filterView, Util.Sort<Data.Entities.ServicesDefault>().Ascending(x => x.Name)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listServices);
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
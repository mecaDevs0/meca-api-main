using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
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
    public class WorkshopServicesService : ApplicationServiceBase<WorkshopServices>, IWorkshopServicesService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<WorkshopServices> _workshopServicesRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<ServicesDefault> _servicesDefaultRepository;
        private readonly IBusinessBaseAsync<Rating> _ratingRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public WorkshopServicesService(
            IMapper mapper,
            IBusinessBaseAsync<WorkshopServices> WorkshopServicesRepository,
            IBusinessBaseAsync<Workshop> WorkshopRepository,
            IBusinessBaseAsync<ServicesDefault> servicesDefaultRepository,
            IBusinessBaseAsync<Rating> ratingRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _workshopServicesRepository = WorkshopServicesRepository;
            _workshopRepository = WorkshopRepository;
            _servicesDefaultRepository = servicesDefaultRepository;
            _ratingRepository = ratingRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public WorkshopServicesService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            string testUnit)
        {
            _workshopServicesRepository = new BusinessBaseAsync<WorkshopServices>(env);
            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
        }

        public async Task<List<WorkshopServicesViewModel>> GetAll()
        {
            var listWorkshopServices = await _workshopServicesRepository.FindAllAsync(Util.Sort<WorkshopServices>().Ascending(nameof(WorkshopServices.Service.Name)));

            return _mapper.Map<List<WorkshopServicesViewModel>>(listWorkshopServices);
        }

        public async Task<List<T>> GetAll<T>(WorkshopServicesFilterViewModel filterView) where T : class
        {
            filterView.SetDefault();
            var builder = Builders<WorkshopServices>.Filter;
            var conditions = new List<FilterDefinition<WorkshopServices>>();
            var workshopList = new List<Workshop>();

            if (filterView.DataBlocked != null)
            {
                conditions.Add(filterView.DataBlocked == FilterActived.Actived
                    ? builder.Eq(x => x.DataBlocked, null)
                    : builder.Ne(x => x.DataBlocked, null));
            }

            if (string.IsNullOrEmpty(filterView.Search) == false)
            {
                var combinedConditions = new List<FilterDefinition<WorkshopServices>>();

                var properties = typeof(WorkshopServices).GetProperties();

                foreach (var property in properties)
                {
                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                        continue;

                    combinedConditions.Add(
                        Builders<WorkshopServices>.Filter.Regex(property.Name, new BsonRegularExpression(filterView.Search, "i"))
                    );
                }

                conditions.Add(builder.Or(combinedConditions));
            }

            if (filterView.ServiceTypes != null)
            {
                // Primeiro, pegamos todos os ServicesDefault e filtramos na aplicação
                var allServicesDefault = await _servicesDefaultRepository.GetCollectionAsync().Find(Builders<ServicesDefault>.Filter.Empty).ToListAsync();

                // Filtramos os serviços que possuem um ID na lista de ServiceTypes
                var listServicesDefault = allServicesDefault.Where(x => filterView.ServiceTypes.Contains(x.GetStringId())).ToList();

                var servicesId = listServicesDefault.Select(s => s.GetStringId()).ToList();

                if (servicesId.Any())
                {
                    var workshopServices = await _workshopServicesRepository.FindByAsync(x => servicesId.Contains(x.Service.Id));
                    var workshopIds = workshopServices.Select(x => x.Workshop.Id).ToList();
                    conditions.Add(builder.In(x => x.Workshop.Id, workshopIds));
                }
            }

            // Task para remover preço do serviço
            // if (filterView.PriceRangeInitial > 0 || filterView.PriceRangeFinal > 0)
            // {
            //     var priceConditions = new List<FilterDefinition<WorkshopServices>>();
            //     if (filterView.PriceRangeInitial > 0) priceConditions.Add(builder.Gte(x => x.Value, filterView.PriceRangeInitial));
            //     if (filterView.PriceRangeFinal > 0) priceConditions.Add(builder.Lte(x => x.Value, filterView.PriceRangeFinal));

            //     conditions.Add(builder.And(priceConditions));
            // }

            if (filterView.Rating != null)
            {
                workshopList = (List<Workshop>)await _workshopRepository.FindByAsync(x => x.Rating == filterView.Rating);
                var workshopIds = workshopList.Select(Workshop => Workshop.GetStringId()).ToList();
                conditions.Add(builder.In(x => x.Workshop.Id, workshopIds));
            }

            if (filterView.WorkshopName != null)
            {
                workshopList = (List<Workshop>)await _workshopRepository.FindByAsync(x => x.CompanyName == filterView.WorkshopName);
                var workshopIds = workshopList.Select(workshop => workshop.GetStringId()).ToList();
                conditions.Add(builder.In(x => x.Workshop.Id, workshopIds));
            }

            if (string.IsNullOrEmpty(filterView.WorkshopId) == false)
            {
                conditions.Add(builder.Eq(x => x.Workshop.Id, filterView.WorkshopId));
            }

            if (string.IsNullOrEmpty(filterView.WorkshopId) == true && (int)_access.TypeToken == (int)TypeProfile.Workshop)
            {
                conditions.Add(builder.Eq(x => x.Workshop.Id, _access.UserId));
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

                workshopList = (List<Workshop>)await _workshopRepository.FindAllAsync();

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

                var workshopIds = responseWithDistance.Select(Workshop => Workshop.GetStringId()).ToList();
                conditions.Add(builder.In(x => x.Workshop.Id, workshopIds));
            }

            var listWorkshopServices = await _workshopServicesRepository
                .GetCollectionAsync()
                .FindSync(
                    builder.And(conditions),
                    Util.FindOptions(filterView, Util.Sort<WorkshopServices>().Ascending(x => x.Service.Name))
                )
                .ToListAsync();

            return _mapper.Map<List<T>>(listWorkshopServices);
        }

        public async Task<WorkshopServicesViewModel> GetById(string id, string latUser, string longUser)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var workshopServicesEntity = await _workshopServicesRepository.FindByIdAsync(id);
                if (workshopServicesEntity == null)
                {
                    CreateNotification(DefaultMessages.ServicesNotFound);
                    return null;
                }

                return _mapper.Map<WorkshopServicesViewModel>(workshopServicesEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<WorkshopServicesViewModel> Register(WorkshopServicesViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var workshopEntity = await _workshopRepository.FindByIdAsync(_access.UserId);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                var serviceExist = await _workshopServicesRepository.FindOneByAsync(x => x.Service.Id == model.Service.Id && x.Workshop.Id == workshopEntity.GetStringId());
                if (serviceExist != null)
                {
                    CreateNotification(DefaultMessages.ServiceInUse);
                    return null;
                }

                var workshopServicesEntity = _mapper.Map<WorkshopServices>(model);

                workshopServicesEntity.Workshop = _mapper.Map<WorkshopAux>(workshopEntity);

                workshopServicesEntity = await _workshopServicesRepository.CreateReturnAsync(workshopServicesEntity);

                return _mapper.Map<WorkshopServicesViewModel>(workshopServicesEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<WorkshopServicesViewModel> UpdatePatch(string id, WorkshopServicesViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                var workshopEntity = await _workshopRepository.FindByIdAsync(_access.UserId);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                var serviceExist = await _workshopServicesRepository.FindOneByAsync(x => x.Service.Id == model.Service.Id && x.Workshop.Id == workshopEntity.GetStringId());
                if (string.IsNullOrEmpty(model.Service.Id) == false && await _workshopServicesRepository.CheckByAsync(x => x.Service.Id == model.Service.Id && x.Workshop.Id != workshopEntity.GetStringId()))
                {
                    CreateNotification(DefaultMessages.ServiceInUse);
                    return null;
                }

                var workshopServicesEntity = await _workshopServicesRepository.FindByIdAsync(id);
                if (workshopServicesEntity == null)
                {
                    CreateNotification(DefaultMessages.ServicesNotFound);
                    return null;
                }

                workshopServicesEntity.SetIfDifferent(model, validOnly, _mapper);

                workshopServicesEntity = await _workshopServicesRepository.UpdateAsync(workshopServicesEntity);

                return _mapper.Map<WorkshopServicesViewModel>(workshopServicesEntity);
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
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return false;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(_access.UserId);

                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return false;
                }

                var workshopServicesEntity = await _workshopServicesRepository.FindByIdAsync(id);

                if (workshopServicesEntity == null)
                {
                    CreateNotification(DefaultMessages.ServicesNotFound);
                    return false;
                }

                if (workshopServicesEntity.Workshop.Id != workshopEntity.GetStringId())
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return false;
                }

                await _workshopServicesRepository.DeleteOneAsync(id);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
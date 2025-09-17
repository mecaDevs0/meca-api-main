using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
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
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;

namespace Meca.ApplicationService.Services
{
    public class VehicleService : ApplicationServiceBase<Vehicle>, IVehicleService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<Vehicle> _vehicleRepository;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly ISenderMailService _senderMailService;
        private readonly ISchedulingService _schedulingService;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private static readonly HttpClient httpClient = new HttpClient();

        public VehicleService(
            IMapper mapper,
            IBusinessBaseAsync<Vehicle> vehicleRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            ISenderMailService senderMailService,
            ISchedulingService schedulingService,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _vehicleRepository = vehicleRepository;
            _profileRepository = profileRepository;
            _schedulingRepository = schedulingRepository;
            _workshopRepository = workshopRepository;
            _senderMailService = senderMailService;
            _schedulingService = schedulingService;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public VehicleService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            IBusinessBaseAsync<Vehicle> vehicleRepository,
            string testUnit)
        {
            _vehicleRepository = vehicleRepository;
            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
        }

        public async Task<List<VehicleViewModel>> GetAll()
        {
            var userId = _access.UserId;
            var listVehicle = await _vehicleRepository.FindByAsync(x => x.Profile.Id == userId, Util.Sort<Vehicle>().Ascending(nameof(Vehicle.Year)));

            return _mapper.Map<List<VehicleViewModel>>(listVehicle);
        }

        public async Task<List<T>> GetAll<T>(VehicleFilterViewModel filterView) where T : class
        {

            filterView.SetDefault();
            var builder = Builders<Vehicle>.Filter;
            var conditions = new List<FilterDefinition<Vehicle>>();

            if ((int)_access.TypeToken == (int)TypeProfile.Profile && string.IsNullOrEmpty(filterView.ProfileId) == true)
            {
                conditions.Add(builder.Eq(x => x.Profile.Id, _access.UserId));
            }

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

            if (string.IsNullOrEmpty(filterView.Plate) == false)
                conditions.Add(builder.Regex(x => x.Plate, new BsonRegularExpression(filterView.Plate, "i")));

            if (string.IsNullOrEmpty(filterView.Manufacturer) == false)
                conditions.Add(builder.Regex(x => x.Manufacturer, new BsonRegularExpression(filterView.Manufacturer, "i")));

            if (string.IsNullOrEmpty(filterView.Model) == false)
                conditions.Add(builder.Regex(x => x.Model, new BsonRegularExpression(filterView.Model, "i")));

            if (string.IsNullOrEmpty(filterView.ProfileId) == false)
            {
                conditions.Add(builder.Eq(x => x.Profile.Id, filterView.ProfileId));
            }

            if (string.IsNullOrEmpty(filterView.Search) == false)
            {
                var search = filterView.Search.Trim();
                var regex = new BsonRegularExpression(new Regex(search, RegexOptions.IgnoreCase));
                conditions.Add(builder.Or(
                    builder.Regex(x => x.Plate, regex),
                    builder.Regex(x => x.Manufacturer, regex),
                    builder.Regex(x => x.Model, regex)
                ));
            }

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listWorkshopServices = await _vehicleRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions(filterView, Util.Sort<Vehicle>().Ascending(x => x.Year)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listWorkshopServices);
        }

        public async Task<VehicleViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var vehicleEntity = await _vehicleRepository.FindByIdAsync(id);

                if (vehicleEntity == null)
                {
                    CreateNotification(DefaultMessages.VehicleNotFound);
                    return null;
                }
                return _mapper.Map<VehicleViewModel>(vehicleEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<VehicleViewModel> Register(VehicleViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var profileEntity = await _profileRepository.FindByIdAsync(_access.UserId);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                var vehicleExist = await _vehicleRepository.FindOneByAsync(x => x.Plate == model.Plate && x.Profile.Id == profileEntity.GetStringId());
                if (vehicleExist != null)
                {
                    CreateNotification(DefaultMessages.VehicleInUse);
                    return null;
                }

                model.Plate = model.Plate.ToUpper();

                var vehicleEntity = _mapper.Map<Vehicle>(model);

                vehicleEntity.Profile = _mapper.Map<ProfileAux>(profileEntity);

                vehicleEntity = await _vehicleRepository.CreateReturnAsync(vehicleEntity);

                return _mapper.Map<VehicleViewModel>(vehicleEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VehicleViewModel> UpdatePatch(string id, VehicleViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                var profileEntity = await _profileRepository.FindByIdAsync(_access.UserId);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                var vehicleExist = await _vehicleRepository.FindOneByAsync(x => x.Plate == model.Plate && x.Profile.Id == profileEntity.GetStringId());
                if (string.IsNullOrEmpty(model.Plate) == false && await _vehicleRepository.CheckByAsync(x => x.Plate == model.Plate && x.Profile.Id != profileEntity.GetStringId()))
                {
                    CreateNotification(DefaultMessages.VehicleInUse);
                    return null;
                }

                var vehicleEntity = await _vehicleRepository.FindByIdAsync(id);
                if (vehicleEntity == null)
                {
                    CreateNotification(DefaultMessages.VehicleNotFound);
                    return null;
                }

                if (string.IsNullOrEmpty(model.Plate) == false)
                {
                    model.Plate = model.Plate.ToUpper();
                }

                var vehicleEntityOld = vehicleEntity;
                vehicleEntity.SetIfDifferent(model, validOnly, _mapper);

                vehicleEntity = await _vehicleRepository.UpdateAsync(vehicleEntity);

                var schedulingList = await _schedulingRepository.FindByAsync(x => x.Vehicle.Id == vehicleEntity.GetStringId() && x.Status != SchedulingStatus.ServiceFinished);

                if (schedulingList.Any())
                {
                    foreach (var schedulingEntity in schedulingList)
                    {
                        string title = "Informações do veículo alteradas";
                        var message = new StringBuilder();
                        message.AppendLine("<p>Olá <strong>{{ name }}</strong>,<br/>");
                        message.AppendLine($"O cliente alterou as informações do veículo referente ao agendamento de n° {schedulingEntity.GetStringId()}.<br/>");
                        message.AppendLine("<p>Informação(ões) modificada(s) abaixo:<br/>");

                        var modifiedFields = GetModifiedFields(vehicleEntityOld, vehicleEntity);

                        foreach (var field in modifiedFields)
                        {
                            string displayName = Util.GetDisplayName(typeof(SchedulingViewModel), field.Key);
                            message.AppendLine($"<p>{displayName} - Valor antigo: {field.Value.OldValue} - Valor novo: {field.Value.NewValue}<br/>");
                        }

                        await _schedulingService.SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);
                    }
                }

                return _mapper.Map<VehicleViewModel>(vehicleEntity);
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

                var profileEntity = await _profileRepository.FindByIdAsync(_access.UserId);

                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return false;
                }

                var vehicleEntity = await _vehicleRepository.FindByIdAsync(id);

                if (vehicleEntity == null)
                {
                    CreateNotification(DefaultMessages.VehicleNotFound);
                    return false;
                }

                if (vehicleEntity.Profile.Id != profileEntity.GetStringId())
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return false;
                }

                await _vehicleRepository.DeleteOneAsync(id);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Dictionary<string, (object OldValue, object NewValue)> GetModifiedFields<T>(T oldObject, T newObject)
        {
            var modifiedFields = new Dictionary<string, (object OldValue, object NewValue)>();

            if (oldObject == null || newObject == null)
                return modifiedFields;

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                object oldValue = property.GetValue(oldObject);
                object newValue = property.GetValue(newObject);

                if (!object.Equals(oldValue, newValue))
                {
                    modifiedFields[property.Name] = (oldValue, newValue);
                }
            }

            return modifiedFields;
        }

        public async Task<VehicleInfoViewModel> GetInfoVehicleByPlate(string plate)
        {
            try
            {
                string token = "2ff462f20ad3d3377a9649f2d8d106d8";
                string url = $"https://wdapi2.com.br/consulta/{plate}/{token}";

                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                dynamic jsonObject = JsonConvert.DeserializeObject(jsonResponse);

                if (jsonObject.erro == true)
                    throw new Exception("Erro na consulta da placa: " + (string)(jsonObject.message ?? "Erro desconhecido"));

                var vehicle = new VehicleInfoViewModel
                {
                    Plate = jsonObject.placa,
                    Manufacturer = jsonObject.MARCA,
                    Model = jsonObject.MODELO,
                    Color = jsonObject.cor,
                    Year = jsonObject.ano,
                };

                return vehicle;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar veículo: " + ex.Message, ex);
            }
        }
    }

}
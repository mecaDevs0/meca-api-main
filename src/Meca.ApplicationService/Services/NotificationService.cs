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
using MongoDB.Driver.Builders;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Infra.Core3.MongoDb.Data.Modelos;
using UtilityFramework.Services.Core3;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Core3.Models;

namespace Meca.ApplicationService.Services
{
    public class NotificationService : ApplicationServiceBase<Notification>, INotificationService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<Notification> _notificationRepository;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<UserAdministrator> _userAdministratorRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly ISenderNotificationService _senderNotificationService;
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public NotificationService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            IBusinessBaseAsync<Notification> notificationRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            string testUnit
            )
        {
            _env = env;
            _notificationRepository = notificationRepository;
            _profileRepository = profileRepository;
            _userAdministratorRepository = userAdministratorRepository;
            _workshopRepository = workshopRepository;
            _mapper = mapper;
            _configuration = configuration;
            _senderNotificationService = new SendService();

            SetAccessTest(acesso);
        }

        public NotificationService(
            IMapper mapper,
            IBusinessBaseAsync<Notification> notificationRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment env,
            IConfiguration configuration,
            ISenderNotificationService senderNotificationService)
        {
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _profileRepository = profileRepository;
            _userAdministratorRepository = userAdministratorRepository;
            _workshopRepository = workshopRepository;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _configuration = configuration;
            _senderNotificationService = senderNotificationService;
        }

        public async Task<List<NotificationViewModel>> GetAll()
        {
            var listEntity = await _notificationRepository.FindAllAsync(Util.Sort<Notification>().Descending(nameof(Notification.Created)));

            return _mapper.Map<List<NotificationViewModel>>(listEntity);
        }

        public async Task<List<T>> GetAll<T>(NotificationFilterViewModel filterView) where T : class
        {

            filterView.SetDefault();

            var builder = Builders<Notification>.Filter;
            var conditions = new List<FilterDefinition<Notification>>();

            if (filterView.TypeReference != null)
                conditions.Add(builder.Eq(x => x.TypeReference, filterView.TypeReference.GetValueOrDefault()));

            if (filterView.UserId != null)
                conditions.Add(builder.Eq(x => x.ReferenceId, filterView.UserId));

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listEntity = await _notificationRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions<Notification>(filterView, Util.Sort<Notification>().Descending(x => x.Created)))
            .ToListAsync();

            if (filterView.SetRead)
            {
                /*MARCA NOTIFICAÇÕES COMO VISUALIZADAS*/

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                _notificationRepository.UpdateMultiple(Query<Notification>.In(x => x._id, listEntity.Select(x => x._id).ToList()),
                    new UpdateBuilder<Notification>()
                    .Set(x => x.DateRead, now)
                    .Set(x => x.LastUpdate, now),
                    UpdateFlags.Multi);
            }

            return _mapper.Map<List<T>>(listEntity);
        }

        public async Task<NotificationViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var notificationEntity = await _notificationRepository.FindByIdAsync(id);

                if (notificationEntity == null)
                {
                    CreateNotification(DefaultMessages.NotificationNotFound);
                    return null;
                }
                return _mapper.Map<NotificationViewModel>(notificationEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendNotification(List<NotificationAux> listTargets, List<string> listDeviceId, string title, string content, dynamic payload, dynamic settings, bool sendPush = true, bool registerNotification = true, string groupName = null)
        {
            try
            {
                if (registerNotification)
                {
                    /*REGISTRA NOTIFICAÇÕES*/
                    var listNotification = _mapper.Map<List<NotificationAux>, List<Notification>>(listTargets,
                     opt => opt.AfterMap((src, dest) =>
                     {

                         for (int i = 0; i < src.Count; i++)
                         {
                             dest[i].Title = title;
                             dest[i].Content = content;
                         }

                     }));

                    await _notificationRepository.CreateAsync(listNotification);
                }

                if (sendPush && listDeviceId != null && listDeviceId.Count > 0)
                {
                    /*ENVIA PUSH VIA ONESIGNAL*/

                    var result = (OneSignalResponse)await _senderNotificationService.SendPushAsync(title, content, listDeviceId, groupName, data: payload, settings: settings, priority: 10);

                    if (_env.EnvironmentName != "Production" && result.Erro)
                        throw new Exception(DefaultMessages.ErrorOnSendPush);
                }

                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendAndRegisterNotification(SendPushViewModel model, WorkshopAux workshop = null, ProfileAux profile = null, string schedulingId = null)
        {

            // if (_access.IsAdmin == false)
            // {
            //     CreateNotification(DefaultMessages.OnlyAdministrator);
            //     return false;
            // }

            List<Notification> listNotification = null;
            List<string> listDeviceId = null;
            try
            {
                if (ModelIsValid(model, true) == false)
                    return false;

                var indexPush = (int)IndexPush.Profile;

                switch (model.TypeProfile)
                {
                    case TypeProfile.UserAdministrator:

                        var listUserAdministrator = await GetEntitiesForNotification<UserAdministrator>(model.TargetId);

                        if (listUserAdministrator.Count > 0)
                        {
                            listNotification = GenericMap(listUserAdministrator, model.Title, model.Content, workshop: workshop, profile: profile, schedulingId: schedulingId);
                        }
                        break;
                    case TypeProfile.Profile:

                        var listProfile = await GetEntitiesForNotification<Data.Entities.Profile>(model.TargetId);

                        if (listProfile.Count > 0)
                        {
                            listNotification = GenericMap(listProfile, model.Title, model.Content, workshop: workshop);
                            listDeviceId = listProfile.SelectMany(x => x.DeviceId).Where(x => string.IsNullOrEmpty(x) == false).Select(x => x).Distinct().ToList();
                        }
                        break;
                    case TypeProfile.Workshop:

                        indexPush = (int)IndexPush.Workshop;

                        var listWorkshop = await GetEntitiesForNotification<Workshop>(model.TargetId);

                        if (listWorkshop.Count > 0)
                        {
                            listNotification = GenericMap(listWorkshop, model.Title, model.Content, profile: profile);
                            listDeviceId = listWorkshop.SelectMany(x => x.DeviceId).Where(x => string.IsNullOrEmpty(x) == false).Select(x => x).Distinct().ToList();
                        }
                        break;
                    default:
                        CreateNotification(DefaultMessages.InvalidTypeProfile);
                        return false;
                }

                if (listNotification != null && listNotification.Count > 0)
                {
                    var saved = 0;
                    var index = 0;
                    while (listNotification.Count > saved)
                    {
                        var toSave = listNotification.Skip((index * 250)).Take(250).ToList();

                        await _notificationRepository.CreateAsync(toSave);
                        saved += toSave.Count;
                        index++;
                    }

                    if (listDeviceId != null && listDeviceId.Count > 0)
                    {
                        dynamic payLoad = Util.GetPayloadPush(RouteNotification.System);

                        dynamic settings = Util.GetSettingsPush();

                        var result = (OneSignalResponse)await _senderNotificationService.SendPushAsync(model.Title, model.Content, listDeviceId, data: payLoad, settings: settings, priority: 10, indexKeys: indexPush);

                        if (_env.EnvironmentName != "Production" && result.Erro)
                            throw new Exception(DefaultMessages.ErrorOnSendPush);
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
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

                await _notificationRepository.DeleteOneAsync(id);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<T>> GetEntitiesForNotification<T>(List<string> targets) where T : ModelBase
        {
            var response = new List<T>();
            try
            {
                IBusinessBaseAsync<T> repository = null;
                if (typeof(T) == typeof(UserAdministrator))
                    repository = _userAdministratorRepository as IBusinessBaseAsync<T>;
                else if (typeof(T) == typeof(Data.Entities.Profile))
                    repository = _profileRepository as IBusinessBaseAsync<T>;
                else if (typeof(T) == typeof(Workshop))
                    repository = _workshopRepository as IBusinessBaseAsync<T>;
                else
                    throw new InvalidOperationException($"Repositório não injetado para o tipo {typeof(T).Name}");

                var builder = Builders<T>.Filter;
                var conditions = new List<FilterDefinition<T>>();

                conditions.Add(builder.Eq(x => x.Disabled, null));
                conditions.Add(builder.Eq(x => x.DataBlocked, null));

                if (targets.Count != 0)
                    conditions.Add(builder.In(x => x._id, targets.Select(ObjectId.Parse).ToList()));

                response = await repository.GetCollectionAsync().Find(builder.And(conditions)).ToListAsync();

                return response;

            }
            catch (Exception)
            {
                return response;
            }
        }

        private List<Notification> GenericMap<T>(List<T> targetsEntity, string title, string content, WorkshopAux workshop = null, ProfileAux profile = null, string schedulingId = null) where T : class
        {
            return _mapper.Map<List<T>, List<Notification>>(targetsEntity, opts => opts.AfterMap((src, dest) =>
            {

                for (int i = 0; i < src.Count; i++)
                {
                    dest[i].Title = title;
                    dest[i].Content = content;

                    if (workshop != null)
                    {
                        dest[i].Workshop = workshop;
                    }

                    if (profile != null)
                    {
                        dest[i].Profile = profile;
                    }

                    if (schedulingId != null)
                    {
                        dest[i].SchedulingId = schedulingId;
                    }
                }
            }));
        }
    }
}
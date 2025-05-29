using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
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
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.ApplicationService.Services
{
    public class SchedulingHistoryService : ApplicationServiceBase<SchedulingHistory>, ISchedulingHistoryService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<SchedulingHistory> _schedulingHistoryRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public SchedulingHistoryService(
            IMapper mapper,
            IBusinessBaseAsync<SchedulingHistory> schedulingHistoryRepository,
            IBusinessBaseAsync<Workshop> WorkshopRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _schedulingHistoryRepository = schedulingHistoryRepository;
            _workshopRepository = WorkshopRepository;
            _profileRepository = profileRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public SchedulingHistoryService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            string testUnit)
        {
            _schedulingHistoryRepository = new BusinessBaseAsync<SchedulingHistory>(env);
            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
        }

        public async Task<List<SchedulingHistoryViewModel>> GetAll()
        {
            var listSchedulingHistory = await _schedulingHistoryRepository.FindAllAsync(Util.Sort<SchedulingHistory>().Ascending(x => x._id));

            return _mapper.Map<List<SchedulingHistoryViewModel>>(listSchedulingHistory);
        }

        public async Task<List<T>> GetAll<T>(SchedulingHistoryFilterViewModel filterView) where T : class
        {

            filterView.SetDefault();
            var builder = Builders<SchedulingHistory>.Filter;
            var conditions = new List<FilterDefinition<SchedulingHistory>>();

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

            if (!string.IsNullOrEmpty(filterView.SchedulingId))
            {
                conditions.Add(builder.Eq(x => x.SchedulingId, filterView.SchedulingId));
            }

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listSchedulingHistory = await _schedulingHistoryRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions(filterView, Util.Sort<SchedulingHistory>().Ascending(x => x._id)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listSchedulingHistory);
        }

        public async Task<SchedulingHistoryViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var schedulingHistoryEntity = await _schedulingHistoryRepository.FindByIdAsync(id);

                if (schedulingHistoryEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingHistoryNotFound);
                    return null;
                }
                return _mapper.Map<SchedulingHistoryViewModel>(schedulingHistoryEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<SchedulingHistoryViewModel> Register(SchedulingHistoryViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var schedulingHistoryEntity = _mapper.Map<SchedulingHistory>(model);

                schedulingHistoryEntity = await _schedulingHistoryRepository.CreateReturnAsync(schedulingHistoryEntity);

                return _mapper.Map<SchedulingHistoryViewModel>(schedulingHistoryEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
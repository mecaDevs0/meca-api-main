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
    public class RatingService : ApplicationServiceBase<Rating>, IRatingService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<Rating> _ratingRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public RatingService(
            IMapper mapper,
            IBusinessBaseAsync<Rating> ratingRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _ratingRepository = ratingRepository;
            _workshopRepository = workshopRepository;
            _profileRepository = profileRepository;
            _schedulingRepository = schedulingRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public RatingService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            string testUnit)
        {
            _ratingRepository = new BusinessBaseAsync<Rating>(env);
            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
        }

        public async Task<List<RatingViewModel>> GetAll()
        {
            var listRating = await _ratingRepository.FindAllAsync(Util.Sort<Rating>().Ascending(x => x._id));

            return _mapper.Map<List<RatingViewModel>>(listRating);
        }

        public async Task<List<T>> GetAll<T>(RatingFilterViewModel filterView) where T : class
        {

            filterView.SetDefault();
            var builder = Builders<Rating>.Filter;
            var conditions = new List<FilterDefinition<Rating>>();

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

            if (string.IsNullOrEmpty(filterView.WorkshopId) == false)
            {
                conditions.Add(builder.Eq(x => x.Workshop.Id, filterView.WorkshopId));
            }

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listRating = await _ratingRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions(filterView, Util.Sort<Rating>().Ascending(x => x._id)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listRating);
        }

        public async Task<RatingViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var ratingEntity = await _ratingRepository.FindByIdAsync(id);

                if (ratingEntity == null)
                {
                    CreateNotification(DefaultMessages.ServicesNotFound);
                    return null;
                }
                return _mapper.Map<RatingViewModel>(ratingEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<RatingViewModel> Register(RatingViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                if (_access.UserId != schedulingEntity.Profile.Id)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var alreadyRating = await _ratingRepository.FindByAsync(x => x.SchedulingId == model.SchedulingId && x.Profile.Id == _access.UserId);
                if (alreadyRating.FirstOrDefault() != null)
                {
                    CreateNotification("Você já avaliou este serviço.");
                    return null;
                }

                var ratingEntity = _mapper.Map<Rating>(model);

                ratingEntity.Workshop = _mapper.Map<WorkshopAux>(schedulingEntity.Workshop);
                ratingEntity.BudgetServices = _mapper.Map<List<BudgetServicesAux>>(schedulingEntity.MaintainedBudgetServices);
                ratingEntity.Profile = _mapper.Map<ProfileAux>(schedulingEntity.Profile);
                ratingEntity.Vehicle = _mapper.Map<VehicleAux>(schedulingEntity.Vehicle);
                ratingEntity.RatingAverage = (ratingEntity.AttendanceQuality + ratingEntity.CostBenefit + ratingEntity.ServiceQuality) / 3;

                ratingEntity = await _ratingRepository.CreateReturnAsync(ratingEntity);

                schedulingEntity.HasEvaluated = true;
                await _schedulingRepository.UpdateOneAsync(schedulingEntity);

                _ = Task.Run(async () =>
                {
                    // Alterando média de avaliações da Oficina
                    var workshopEntity = await _workshopRepository.FindByIdAsync(schedulingEntity.Workshop.Id);
                    var allRating = await _ratingRepository.FindByAsync(x => x.Workshop.Id == workshopEntity.GetStringId());
                    var rating = allRating.Sum(x => x.RatingAverage);
                    workshopEntity.Rating = rating / allRating.Count();
                    await _workshopRepository.UpdateAsync(workshopEntity);
                });

                return _mapper.Map<RatingViewModel>(ratingEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<RatingViewModel> UpdatePatch(string id, RatingViewModel model)
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

                var ratingEntity = await _ratingRepository.FindByIdAsync(id);
                if (ratingEntity == null)
                {
                    CreateNotification(DefaultMessages.RatingNotFound);
                    return null;
                }

                ratingEntity.SetIfDifferent(model, validOnly, _mapper);

                ratingEntity = await _ratingRepository.UpdateAsync(ratingEntity);

                return _mapper.Map<RatingViewModel>(ratingEntity);
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

                var ratingEntity = await _ratingRepository.FindByIdAsync(id);

                if (ratingEntity == null)
                {
                    CreateNotification(DefaultMessages.RatingNotFound);
                    return false;
                }

                if (ratingEntity.Profile.Id != profileEntity.GetStringId())
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return false;
                }

                await _ratingRepository.DeleteOneAsync(id);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
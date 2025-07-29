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
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.ApplicationService.Services
{
    public class FaqService : ApplicationServiceBase<Faq>, IFaqService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<Faq> _faqRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public FaqService(
            IMapper mapper,
            IBusinessBaseAsync<Faq> faqRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _faqRepository = faqRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public FaqService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            IBusinessBaseAsync<Faq> faqRepository,
            string testUnit)
        {
            _faqRepository = faqRepository;
            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
        }

        public async Task<List<FaqViewModel>> GetAll()
        {
            var listFaq = await _faqRepository.FindAllAsync(Util.Sort<Faq>().Ascending(x => x._id));

            return _mapper.Map<List<FaqViewModel>>(listFaq);
        }

        public async Task<List<T>> GetAll<T>(FaqFilterViewModel filterView) where T : class
        {

            filterView.SetDefault();
            var builder = Builders<Faq>.Filter;
            var conditions = new List<FilterDefinition<Faq>>();

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

            if (string.IsNullOrEmpty(filterView.Question) == false)
            {
                conditions.Add(builder.Eq(x => x.Question, filterView.Question));
            }

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listFaq = await _faqRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions(filterView, Util.Sort<Faq>().Ascending(x => x._id)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listFaq);
        }

        public async Task<FaqViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var faqEntity = await _faqRepository.FindByIdAsync(id);

                if (faqEntity == null)
                {
                    CreateNotification(DefaultMessages.FaqNotFound);
                    return null;
                }
                return _mapper.Map<FaqViewModel>(faqEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<FaqViewModel> Register(FaqViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                if ((int)_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var faqExist = await _faqRepository.FindOneByAsync(x => x.Question == model.Question);
                if (faqExist != null)
                {
                    CreateNotification(DefaultMessages.FaqInUse);
                    return null;
                }

                var faqEntity = _mapper.Map<Faq>(model);
                faqEntity = await _faqRepository.CreateReturnAsync(faqEntity);

                return _mapper.Map<FaqViewModel>(faqEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FaqViewModel> UpdatePatch(string id, FaqViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                if ((int)_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var faqEntity = await _faqRepository.FindByIdAsync(id);
                if (faqEntity == null)
                {
                    CreateNotification(DefaultMessages.FaqNotFound);
                    return null;
                }

                var faqExist = await _faqRepository.FindOneByAsync(x => x.Question == model.Question);
                if (faqExist != null)
                {
                    CreateNotification(DefaultMessages.FaqInUse);
                    return null;
                }

                faqEntity.SetIfDifferent(model, validOnly, _mapper);

                faqEntity = await _faqRepository.UpdateAsync(faqEntity);

                return _mapper.Map<FaqViewModel>(faqEntity);
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

                if ((int)_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                var faqEntity = await _faqRepository.FindByIdAsync(id);
                if (faqEntity == null)
                {
                    CreateNotification(DefaultMessages.FaqNotFound);
                    return false;
                }

                await _faqRepository.DeleteOneAsync(id);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
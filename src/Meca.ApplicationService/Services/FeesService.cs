using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.ApplicationService.Services;
using Meca.Data.Entities;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.ApplicationService.Services
{

    public class FeesService : ApplicationServiceBase<Fees>, IFeesService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<Fees> _feesRepository;
        private readonly IMapper _mapper;

        public FeesService(
           IMapper mapper,
           IBusinessBaseAsync<Fees> feesRepository,
           IConfiguration configuration)
        {
            _mapper = mapper;
            _feesRepository = feesRepository;
            _configuration = configuration;
        }

        public async Task<FeesViewModel> GetFees()
        {
            try
            {
                var feesList = await _feesRepository.FindAllAsync();

                var feesEntity = feesList.FirstOrDefault();

                if (feesEntity == null)
                {
                    feesEntity = new Fees()
                    {
                        PlatformFee = 0.0
                    };

                    await _feesRepository.CreateAsync(feesEntity);
                }

                return _mapper.Map<FeesViewModel>(feesEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FeesViewModel> RegisterOrUpdate(FeesViewModel model)
        {
            try
            {
                if (ModelIsValid(model) == false)
                    return null;

                Fees feesEntity = null;

                if (string.IsNullOrEmpty(model.Id))
                {
                    feesEntity = _mapper.Map<Fees>(model);
                    feesEntity = await _feesRepository.CreateReturnAsync(feesEntity);

                    _ = Task.Run(async () =>
                    {
                        await _feesRepository.DeleteAsync(src => src._id != feesEntity._id);
                    });
                }
                else
                {
                    feesEntity = await _feesRepository.FindByIdAsync(model.Id);

                    if (feesEntity == null)
                    {
                        CreateNotification(DefaultMessages.FeesNotFound);
                        return null;
                    }

                    feesEntity.SetIfDifferent(model, _jsonBodyFields, _mapper);
                    feesEntity = await _feesRepository.UpdateAsync(feesEntity);
                }

                return _mapper.Map<FeesViewModel>(feesEntity);

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
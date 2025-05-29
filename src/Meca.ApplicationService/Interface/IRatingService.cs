using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface IRatingService : IService
    {
        Task<RatingViewModel> GetById(string id);
        Task<List<RatingViewModel>> GetAll();
        Task<List<T>> GetAll<T>(RatingFilterViewModel filterView) where T : class;
        Task<RatingViewModel> Register(RatingViewModel model);
        Task<RatingViewModel> UpdatePatch(string id, RatingViewModel model);
        Task<bool> Delete(string id);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface IFaqService : IService
    {
        Task<FaqViewModel> GetById(string id);
        Task<List<FaqViewModel>> GetAll();
        Task<List<T>> GetAll<T>(FaqFilterViewModel filterView) where T : class;
        Task<FaqViewModel> Register(FaqViewModel model);
        Task<FaqViewModel> UpdatePatch(string id, FaqViewModel model);
        Task<bool> Delete(string id);
    }
}
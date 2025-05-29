using System.Collections.Generic;
using System.Threading.Tasks;

using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface IAccessLevelService : IService
    {
        Task<AccessLevelViewModel> GetById(string id);
        Task<List<AccessLevelViewModel>> GetAll();
        Task<List<T>> GetAll<T>(AccessLevelFilterViewModel filterView) where T : class;
        Task<AccessLevelViewModel> Register(AccessLevelViewModel model);
        Task<AccessLevelViewModel> UpdatePatch(string id, AccessLevelViewModel model);
        Task<bool> Delete(string id);
        Task<AccessLevelViewModel> RegisterDefault();
        Task<bool> RefreshMenu();
    }
}
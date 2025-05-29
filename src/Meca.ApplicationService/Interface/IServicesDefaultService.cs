using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface IServicesDefaultService : IService
    {
        Task<ServicesDefaultViewModel> GetById(string id);
        Task<List<ServicesDefaultViewModel>> GetAll();
        Task<List<T>> GetAll<T>(ServicesDefaultFilterViewModel filterView) where T : class;
        Task<ServicesDefaultViewModel> Register(ServicesDefaultViewModel model);
        Task<ServicesDefaultViewModel> UpdatePatch(string id, ServicesDefaultViewModel model);
        Task<bool> Delete(string id);
    }
}
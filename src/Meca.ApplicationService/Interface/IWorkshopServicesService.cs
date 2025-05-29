using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface IWorkshopServicesService : IService
    {
        Task<WorkshopServicesViewModel> GetById(string id, string latUser, string longUser);
        Task<List<WorkshopServicesViewModel>> GetAll();
        Task<List<T>> GetAll<T>(WorkshopServicesFilterViewModel filterView) where T : class;
        Task<WorkshopServicesViewModel> Register(WorkshopServicesViewModel model);
        Task<WorkshopServicesViewModel> UpdatePatch(string id, WorkshopServicesViewModel model);
        Task<bool> Delete(string id);
    }
}
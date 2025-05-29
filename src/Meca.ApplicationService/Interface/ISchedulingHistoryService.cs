using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface ISchedulingHistoryService : IService
    {
        Task<SchedulingHistoryViewModel> GetById(string id);
        Task<List<SchedulingHistoryViewModel>> GetAll();
        Task<List<T>> GetAll<T>(SchedulingHistoryFilterViewModel filterView) where T : class;
        Task<SchedulingHistoryViewModel> Register(SchedulingHistoryViewModel model);
    }
}
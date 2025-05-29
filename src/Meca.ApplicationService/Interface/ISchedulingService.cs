using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface ISchedulingService : IService
    {
        Task<SchedulingViewModel> GetById(string id);
        Task<List<SchedulingViewModel>> GetAll();
        Task<List<T>> GetAll<T>(SchedulingFilterViewModel filterView) where T : class;
        Task<SchedulingViewModel> Register(SchedulingViewModel model);
        Task<SchedulingViewModel> UpdatePatch(string id, SchedulingViewModel model);
        Task<bool> Delete(string id);
        Task<CalendarAvailableViewModel> GetAvailableScheduling(SchedulingAvailableFilterViewModel filterModel);
        Task<bool> SuggestNewTime(SuggestNewTimeViewModel model);
        Task<SchedulingViewModel> ConfirmScheduling(ConfirmSchedulingViewModel model);
        Task<SchedulingViewModel> ChangeSchedulingStatus(ChangeSchedulingStatusViewModel model);
        Task<SchedulingViewModel> SendBudget(SendBudgetViewModel model);
        Task<SchedulingViewModel> ConfirmBudget(ConfirmBudgetViewModel model);
        Task<SchedulingViewModel> ConfirmService(ConfirmServiceViewModel model);
        Task<bool> SuggestFreeRepair(string schedulingId);
        Task<SchedulingViewModel> RegisterRepairAppointment(SchedulingViewModel model);
        Task<bool> DisputeDisapprovedService(DisputeDisapprovedServiceViewModel model);
        Task SendProfileNotification(string title, StringBuilder message, string profileId, WorkshopAux workshop);
        Task SendWorkshopNotification(string title, StringBuilder message, string workshopId, ProfileAux profile);
        Task<bool> ApproveOrReproveService(ApproveOrReproveServiceViewModel model);
        Task RegisterSchedulingHistory(Scheduling schedulingEntity);
    }
}
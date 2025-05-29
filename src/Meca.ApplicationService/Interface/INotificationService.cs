using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Data.Entities.Auxiliaries;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;


namespace Meca.ApplicationService.Interface
{
    public interface INotificationService : IService
    {
        Task<NotificationViewModel> GetById(string id);
        Task<List<NotificationViewModel>> GetAll();
        Task<List<T>> GetAll<T>(NotificationFilterViewModel filterView) where T : class;
        Task<bool> SendAndRegisterNotification(SendPushViewModel model, WorkshopAux workshop = null, ProfileAux profile = null, string schedulingId = null);
        Task SendNotification(List<NotificationAux> listTargets, List<string> listDeviceId, string title, string content, dynamic payload, dynamic settings, bool sendPush = true, bool registerNotification = true, string groupName = null);
        Task<bool> Delete(string id);
    }
}
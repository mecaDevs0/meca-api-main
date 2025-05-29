using System.Threading.Tasks;
using Meca.Domain.ViewModels;

namespace Meca.ApplicationService.Interface
{
    public interface IWorkshopAgendaService : IService
    {
        Task<WorkshopAgendaViewModel> GetWorkshopAgenda(string id = null);
        Task<WorkshopAgendaViewModel> RegisterOrUpdate(WorkshopAgendaViewModel model);
        Task<bool> RemoveHour(string date);
    }
}
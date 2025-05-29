
using System.Threading.Tasks;
using Hangfire.Server;

namespace Meca.ApplicationService.Services.Hangfire.Interface
{
    public interface IHangfireService
    {
        Task NotifyScheduling(PerformContext context = null);
        Task RemoveAgenda(PerformContext context = null);
    }
}
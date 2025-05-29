using System.Threading.Tasks;
using Meca.Domain.ViewModels;

namespace Meca.ApplicationService.Interface
{
    public interface IFeesService : IService
    {
        Task<FeesViewModel> GetFees();
        Task<FeesViewModel> RegisterOrUpdate(FeesViewModel model);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;

namespace Meca.ApplicationService.Interface
{
    public interface IVehicleService : IService
    {
        Task<VehicleViewModel> GetById(string id);
        Task<List<VehicleViewModel>> GetAll();
        Task<List<T>> GetAll<T>(VehicleFilterViewModel filterView) where T : class;
        Task<VehicleViewModel> Register(VehicleViewModel model);
        Task<VehicleViewModel> UpdatePatch(string id, VehicleViewModel model);
        Task<bool> Delete(string id);
        Task<VehicleInfoViewModel> GetInfoVehicleByPlate(string plate);
    }
}
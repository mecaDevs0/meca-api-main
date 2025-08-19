using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.ApplicationService.Interface
{
    public interface IWorkshopService : IService
    {
        Task<List<WorkshopViewModel>> GetAll();
        Task<List<T>> GetAll<T>(WorkshopFilterViewModel filterView) where T : class;
        Task<WorkshopViewModel> Detail(string id, string latUser, string longUser);
        Task<WorkshopViewModel> GetInfo();
        Task<bool> Delete(string id);
        Task<bool> DeleteByEmail(string email);
        Task<WorkshopViewModel> Register(WorkshopRegisterViewModel model);
        Task<object> Token(LoginViewModel model);
        Task<string> BlockUnBlock(BlockViewModel model);
        Task<bool> ChangePassword(ChangePasswordViewModel model);
        Task<DtResult<WorkshopViewModel>> LoadData(DtParameters model, WorkshopFilterViewModel filterView);
        Task<bool> RegisterUnRegisterDeviceId(PushViewModel model);
        Task<WorkshopViewModel> UpdatePatch(string id, WorkshopRegisterViewModel model);
        Task<bool> ForgotPassword(LoginViewModel model);
        Task<bool> CheckEmail(ValidationViewModel model);
        Task<bool> CheckCnpj(ValidationViewModel model);
        Task<string> CheckAll(ValidationViewModel model);
        Task<string> UpdateDataBank(DataBankViewModel model, string id);
        Task<DataBankViewModel> GetDataBank(string id);
        Task DeleteStripe(string id);
        Task<DtResult<WorkshopViewModel>> LoadDataGrid(DtParameters model, WorkshopFilterViewModel filterView);
        Task<bool> UpdateWorkshopsWithoutPhotoAndReason();
    }
}
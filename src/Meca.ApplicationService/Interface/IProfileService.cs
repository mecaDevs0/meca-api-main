using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.ApplicationService.Interface
{
    public interface IProfileService : IService
    {
        Task<List<ProfileViewModel>> GetAll();
        Task<List<T>> GetAll<T>(ProfileFilterViewModel filterView) where T : class;
        Task<ProfileViewModel> Detail(string id);
        Task<ProfileViewModel> GetInfo();
        Task<bool> Delete(string id);
        Task<bool> DeleteByEmail(string email);
        Task<bool> SendEmailRequestingAppHelp(SiteMecaFormViewModel model);
        Task<object> Register(ProfileRegisterViewModel model);
        Task<object> Token(LoginViewModel model);
        Task<string> BlockUnBlock(BlockViewModel model);
        Task<bool> ChangePassword(ChangePasswordViewModel model);
        Task<DtResult<ProfileViewModel>> LoadData(DtParameters model);
        Task<bool> RegisterUnRegisterDeviceId(PushViewModel model);
        Task<ProfileViewModel> UpdatePatch(string id, ProfileRegisterViewModel model);
        Task<bool> ForgotPassword(LoginViewModel model);
        Task<bool> CheckEmail(ValidationViewModel model);
        Task<bool> CheckLogin(ValidationViewModel model);
        Task<bool> CheckCpf(ValidationViewModel model);
        Task<string> CheckAll(ValidationViewModel model);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Meca.Data.Enum;
using UtilityFramework.Services.Iugu.Core3.Models;

namespace Meca.ApplicationService.Interface
{
    public interface IFinancialHistoryService : IService
    {
        Task<FinancialHistoryViewModel> GetById(string id);
        Task<List<FinancialHistoryViewModel>> GetAll();
        Task<List<T>> GetAll<T>(FinancialHistoryFilterViewModel filterView) where T : class;
        Task<bool> Canceled(string invoiceId);
        Task<bool> Expired(string invoiceId);
        Task<bool> ConfirmPayment(string invoiceId, PaymentStatus paymentStatus = PaymentStatus.Paid, IuguTriggerModel model = null);
        Task<bool> Delete(string id);
        Task<TransactionViewModel> Payment(PaymentViewModel model);
        Task<bool> PaymentFailed(string invoiceId);
    }
}

using System.Threading.Tasks;
using Meca.Data.Enum;
using Meca.Domain.ViewModels;
using UtilityFramework.Services.Iugu.Core3.Entity;

namespace Meca.Domain.Services.Interface
{
    public interface IUtilService
    {
        string GetFlag(string flag);
        Task<bool> Transfer(RecipientType recipientType, string invoiceId, string destinationAccountId, string destinationLiveKey, double value, double sourceValue, double fees, string sourceLiveKey = null);
        Task<TransactionViewModel> GenerateBankSlip(PayerModel payer, string accountKey, double price, string description, int dueDateDays = 1, string payableWith = "bank_slip");
        Task<TransactionViewModel> TransactionCreditCard(PayerModel payer, string accountKey, string tokenCard, double price, string description, int installment);
    }
}
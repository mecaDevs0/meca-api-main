using System.Threading.Tasks;
using Meca.Domain.ViewModels;

namespace Meca.ApplicationService.Interface
{
    public interface IPagBankService
    {
        Task<PagBankOrderResponse> CreateOrderAsync(PagBankOrderRequest request);
        Task<PagBankOrderDetails> GetOrderDetailsAsync(string orderId);
        Task<PagBankPaymentResponse> ProcessPaymentAsync(PagBankPaymentRequest request);
        Task<bool> CancelOrderAsync(string orderId);
        Task<PagBankWebhookResponse> ProcessWebhookAsync(string payload, string signature);
    }
}

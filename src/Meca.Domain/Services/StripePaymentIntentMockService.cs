using System;
using System.Threading.Tasks;
using Stripe;
using UtilityFramework.Services.Stripe.Core3.Interfaces;
using UtilityFramework.Services.Stripe.Core3.Models;

namespace Meca.Domain.Services
{
    /// <summary>
    /// Mock Service para substituir Stripe durante a migração para PagBank
    /// </summary>
    public class StripePaymentIntentMockService : IStripePaymentIntentService
    {
        public Task<StripeBaseResponse<PaymentIntent>> GetByIdAsync(string id)
        {
            Console.WriteLine($"[STRIPE_MOCK] GetByIdAsync chamado - id: {id}");
            Console.WriteLine($"[STRIPE_MOCK] ATENÇÃO: Stripe foi desabilitado. Use PagBank para pagamentos.");
            
            return Task.FromResult(new StripeBaseResponse<PaymentIntent>
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais.",
                Data = null
            });
        }

        public Task<StripeBaseResponse<StripePaymentIntentDetails>> GetPaymentIntentDetailsAsync(string paymentIntentId)
        {
            Console.WriteLine($"[STRIPE_MOCK] GetPaymentIntentDetailsAsync chamado - paymentIntentId: {paymentIntentId}");
            return Task.FromResult(new StripeBaseResponse<StripePaymentIntentDetails>
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais.",
                Data = null
            });
        }

        public Task<StripeBaseResponse<PaymentIntent>> CreateAnonymousCreditCardPaymentAsync(StripeAnonymousPaymentRequest request)
        {
            Console.WriteLine($"[STRIPE_MOCK] CreateAnonymousCreditCardPaymentAsync chamado");
            Console.WriteLine($"[STRIPE_MOCK] ATENÇÃO: Stripe foi desabilitado. Use PagBank para pagamentos.");
            
            return Task.FromResult(new StripeBaseResponse<PaymentIntent>
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais.",
                Data = null
            });
        }

        public Task<StripeBaseResponse<PaymentIntent>> CreateCreditCardPaymentAsync(StripeTransactionCreditCardRequest request)
        {
            Console.WriteLine($"[STRIPE_MOCK] CreateCreditCardPaymentAsync chamado");
            Console.WriteLine($"[STRIPE_MOCK] ATENÇÃO: Stripe foi desabilitado. Use PagBank para pagamentos.");
            
            return Task.FromResult(new StripeBaseResponse<PaymentIntent>
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais.",
                Data = null
            });
        }

        public Task<StripeBaseResponse<PaymentIntent>> PixPaymentAsync(StripePixPaymentRequest request)
        {
            Console.WriteLine($"[STRIPE_MOCK] PixPaymentAsync chamado");
            return Task.FromResult(new StripeBaseResponse<PaymentIntent>
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais.",
                Data = null
            });
        }

        public Task<StripeBaseResponse> BankSlipPaymentAsync(StripeBankSlipPaymentRequest request)
        {
            Console.WriteLine($"[STRIPE_MOCK] BankSlipPaymentAsync chamado");
            return Task.FromResult(new StripeBaseResponse
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais."
            });
        }

        public Task<StripeBaseResponse> RefundPaymentAsync(string paymentIntentId, long? amount = null)
        {
            Console.WriteLine($"[STRIPE_MOCK] RefundPaymentAsync chamado - paymentIntentId: {paymentIntentId}");
            return Task.FromResult(new StripeBaseResponse
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais."
            });
        }

        public Task<StripeBaseResponse<PaymentIntent>> CapturePaymentIntentAsync(string paymentIntentId, long? amountToCapture = null)
        {
            Console.WriteLine($"[STRIPE_MOCK] CapturePaymentIntentAsync chamado - paymentIntentId: {paymentIntentId}");
            return Task.FromResult(new StripeBaseResponse<PaymentIntent>
            {
                Success = false,
                ErrorMessage = "Stripe foi desabilitado. Use PagBank para pagamentos reais.",
                Data = null
            });
        }
    }
}


using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Meca.ApplicationService.Services
{
    public class PagBankPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _baseUrl;

        public PagBankPaymentService(IHostingEnvironment env, IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            
            // Configuração baseada na documentação do PagBank
            _baseUrl = env.IsProduction() 
                ? "https://api.pagseguro.com" 
                : "https://sandbox.api.pagseguro.com";
            
            _accessToken = configuration["PagBank:AccessToken"] ?? 
                          Environment.GetEnvironmentVariable("PAGBANK_ACCESS_TOKEN") ??
                          "987fa5bc-900b-4903-8172-20f83def1c8b7192a70d4be7bbdd8d2763bff311c2fd93ae-8354-4998-84e6-b5262574d9bd";
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        // Criar pedido de pagamento
        public async Task<PagBankResponse<PagBankOrder>> CreateOrderAsync(PagBankOrderRequest request)
        {
            return await HandleActionAsync(async () =>
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpResponse = await _httpClient.PostAsync($"{_baseUrl}/orders", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                return JsonSerializer.Deserialize<PagBankOrder>(responseContent);
            });
        }

        // Processar pagamento
        public async Task<PagBankResponse<PagBankPayment>> ProcessPaymentAsync(string orderId, PagBankPaymentRequest request)
        {
            return await HandleActionAsync(async () =>
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpResponse = await _httpClient.PostAsync($"{_baseUrl}/orders/{orderId}/payments", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                return JsonSerializer.Deserialize<PagBankPayment>(responseContent);
            });
        }

        // Obter status do pagamento
        public async Task<PagBankResponse<PagBankPaymentStatus>> GetPaymentStatusAsync(string paymentId)
        {
            return await HandleActionAsync(async () =>
            {
                var httpResponse = await _httpClient.GetAsync($"{_baseUrl}/payments/{paymentId}");
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                return JsonSerializer.Deserialize<PagBankPaymentStatus>(responseContent);
            });
        }

        private async Task<PagBankResponse<T>> HandleActionAsync<T>(Func<Task<T>> action)
        {
            var response = new PagBankResponse<T>();

            try
            {
                var result = await action();
                response.Success = true;
                response.Data = result;
            }
            catch (HttpRequestException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"[PagBank HTTP Error] {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
            }

            return response;
        }
    }

    // Modelos para pagamentos
    public class PagBankOrder
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PagBankPayment> Payments { get; set; }
    }

    public class PagBankOrderRequest
    {
        public string ReferenceId { get; set; }
        public PagBankCustomer Customer { get; set; }
        public List<PagBankItem> Items { get; set; }
        public PagBankShipping Shipping { get; set; }
        public List<PagBankQrCode> QrCodes { get; set; }
        public List<PagBankCharges> Charges { get; set; }
        public PagBankNotificationUrls NotificationUrls { get; set; }
    }

    public class PagBankPayment
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PagBankPaymentRequest
    {
        public string PaymentMethod { get; set; }
        public PagBankCreditCard CreditCard { get; set; }
        public PagBankPix Pix { get; set; }
        public PagBankBoleto Boleto { get; set; }
    }

    public class PagBankPaymentStatus
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Modelos auxiliares
    public class PagBankCustomer
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string TaxId { get; set; }
        public PagBankPhone Phone { get; set; }
    }

    public class PagBankItem
    {
        public string ReferenceId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitAmount { get; set; }
    }

    public class PagBankShipping
    {
        public PagBankAddress Address { get; set; }
    }

    public class PagBankAddress
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public string Locality { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string RegionCode { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    public class PagBankPhone
    {
        public string Country { get; set; }
        public string Area { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
    }

    public class PagBankQrCode
    {
        public decimal Amount { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class PagBankCharges
    {
        public string ReferenceId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public PagBankPaymentMethod PaymentMethod { get; set; }
    }

    public class PagBankPaymentMethod
    {
        public string Type { get; set; }
        public int Installments { get; set; }
        public bool Capture { get; set; }
        public PagBankCreditCard CreditCard { get; set; }
        public PagBankPix Pix { get; set; }
        public PagBankBoleto Boleto { get; set; }
    }

    public class PagBankCreditCard
    {
        public string EncryptedCard { get; set; }
        public string SecurityCode { get; set; }
        public PagBankCardHolder CardHolder { get; set; }
        public bool Store { get; set; }
    }

    public class PagBankCardHolder
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string TaxId { get; set; }
        public PagBankPhone Phone { get; set; }
        public PagBankAddress Address { get; set; }
    }

    public class PagBankPix
    {
        public DateTime ExpirationDate { get; set; }
        public List<PagBankAdditionalInformation> AdditionalInformation { get; set; }
    }

    public class PagBankBoleto
    {
        public DateTime DueDate { get; set; }
        public PagBankBoletoInstructionLines InstructionLines { get; set; }
        public PagBankBoletoHolder Holder { get; set; }
    }

    public class PagBankAdditionalInformation
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class PagBankBoletoInstructionLines
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
    }

    public class PagBankBoletoHolder
    {
        public string Name { get; set; }
        public string TaxId { get; set; }
        public string Email { get; set; }
        public PagBankAddress Address { get; set; }
    }

    public class PagBankNotificationUrls
    {
        public List<string> Webhooks { get; set; }
    }
}



using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Meca.ApplicationService.Interface;
using Meca.Domain.ViewModels;
using UtilityFramework.Application.Core3;

namespace Meca.ApplicationService.Services
{
    public class PagBankService : IPagBankService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PagBankService> _logger;
        private readonly string _accessToken;
        private readonly string _baseUrl;
        private readonly bool _isSandbox;

        public PagBankService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PagBankService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _accessToken = _configuration["PAGBANK_ACCESS_TOKEN"];
            _isSandbox = _configuration.GetValue<bool>("PAGBANK_SANDBOX", false);
            _baseUrl = _isSandbox 
                ? "https://sandbox.api.pagseguro.com" 
                : "https://api.pagseguro.com";

            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new InvalidOperationException("PAGBANK_ACCESS_TOKEN não configurado");
            }

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<PagBankOrderResponse> CreateOrderAsync(PagBankOrderRequest request)
        {
            try
            {
                _logger.LogInformation("Criando pedido no PagBank para referência: {ReferenceId}", request.ReferenceId);

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/orders", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao criar pedido no PagBank. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    throw new HttpRequestException($"Erro ao criar pedido: {response.StatusCode} - {responseContent}");
                }

                var orderResponse = JsonSerializer.Deserialize<PagBankOrderResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Pedido criado com sucesso no PagBank. ID: {OrderId}", orderResponse.Id);
                return orderResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido no PagBank");
                throw;
            }
        }

        public async Task<PagBankOrderDetails> GetOrderDetailsAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("Buscando detalhes do pedido no PagBank: {OrderId}", orderId);

                var response = await _httpClient.GetAsync($"/orders/{orderId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao buscar pedido no PagBank. Status: {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"Erro ao buscar pedido: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var orderDetails = JsonSerializer.Deserialize<PagBankOrderDetails>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Detalhes do pedido obtidos com sucesso: {OrderId}", orderId);
                return orderDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do pedido no PagBank: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<PagBankPaymentResponse> ProcessPaymentAsync(PagBankPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processando pagamento no PagBank. OrderId: {OrderId}, PaymentMethodId: {PaymentMethodId}", 
                    request.OrderId, request.PaymentMethodId);

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/orders/{request.OrderId}/payments", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao processar pagamento no PagBank. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    throw new HttpRequestException($"Erro ao processar pagamento: {response.StatusCode} - {responseContent}");
                }

                var paymentResponse = JsonSerializer.Deserialize<PagBankPaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Pagamento processado com sucesso no PagBank. PaymentId: {PaymentId}", paymentResponse.Id);
                return paymentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento no PagBank");
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("Cancelando pedido no PagBank: {OrderId}", orderId);

                var response = await _httpClient.PostAsync($"/orders/{orderId}/cancel", null);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao cancelar pedido no PagBank. Status: {StatusCode}", response.StatusCode);
                    return false;
                }

                _logger.LogInformation("Pedido cancelado com sucesso no PagBank: {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pedido no PagBank: {OrderId}", orderId);
                return false;
            }
        }

        public async Task<PagBankWebhookResponse> ProcessWebhookAsync(string payload, string signature)
        {
            try
            {
                _logger.LogInformation("Processando webhook do PagBank");

                // TODO: Implementar validação de assinatura do webhook
                // var isValidSignature = ValidateWebhookSignature(payload, signature);
                // if (!isValidSignature)
                // {
                //     throw new UnauthorizedAccessException("Assinatura do webhook inválida");
                // }

                var webhookResponse = JsonSerializer.Deserialize<PagBankWebhookResponse>(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Webhook processado com sucesso. Event: {Event}, OrderId: {OrderId}", 
                    webhookResponse.Event, webhookResponse.Data?.Id);

                return webhookResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook do PagBank");
                throw;
            }
        }

        private bool ValidateWebhookSignature(string payload, string signature)
        {
            // TODO: Implementar validação de assinatura do webhook
            // Esta implementação depende da documentação específica do PagBank
            return true;
        }
    }
}







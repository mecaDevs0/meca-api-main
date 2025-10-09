using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Meca.ApplicationService.Services
{
    public class PagBankService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _baseUrl;

        public PagBankService(IHostingEnvironment env, IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            
            // Configuração baseada na documentação do PagBank
            _baseUrl = env.IsProduction() 
                ? "https://api.pagseguro.com" 
                : "https://sandbox.api.pagseguro.com";
            
            _accessToken = configuration["PagBank:AccessToken"] ?? 
                          Environment.GetEnvironmentVariable("PAGBANK_ACCESS_TOKEN") ??
                          "987fa5bc-900b-4903-8172-20f83def1c8b7192a70d4be7bbdd8d2763bff311c2fd93ae-8354-4998-84e6-b5262574d9bd";
            
            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new Exception("PagBank:AccessToken não encontrada no appsettings.{env}.json");
            }
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        public async Task<PagBankResponse<T>> HandleActionAsync<T>(
            Func<Task<T>> action)
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

        // Criar conta (equivalente ao CreateAsync do Stripe)
        public async Task<PagBankResponse<PagBankAccount>> CreateAccountAsync(PagBankAccountRequest request)
        {
            return await HandleActionAsync(async () =>
            {
                // Baseado na documentação do PagBank - API de Cadastro
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpResponse = await _httpClient.PostAsync($"{_baseUrl}/accounts", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                return JsonSerializer.Deserialize<PagBankAccount>(responseContent);
            });
        }

        // Buscar conta por ID (equivalente ao GetByIdAsync do Stripe)
        public async Task<PagBankResponse<PagBankAccount>> GetAccountByIdAsync(string accountId)
        {
            return await HandleActionAsync(async () =>
            {
                var httpResponse = await _httpClient.GetAsync($"{_baseUrl}/accounts/{accountId}");
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                return JsonSerializer.Deserialize<PagBankAccount>(responseContent);
            });
        }

        // Atualizar conta (equivalente ao UpdateAsync do Stripe)
        public async Task<PagBankResponse<PagBankAccount>> UpdateAccountAsync(string accountId, PagBankAccountRequest request)
        {
            return await HandleActionAsync(async () =>
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpResponse = await _httpClient.PutAsync($"{_baseUrl}/accounts/{accountId}", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                return JsonSerializer.Deserialize<PagBankAccount>(responseContent);
            });
        }

        // Criar conta bancária (equivalente ao CreateBankAccountOptions do Stripe)
        public async Task<PagBankResponse<PagBankBankAccount>> CreateBankAccountAsync(string accountId, PagBankBankAccountRequest request)
        {
            return await HandleActionAsync(async () =>
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpResponse = await _httpClient.PostAsync($"{_baseUrl}/accounts/{accountId}/bank-accounts", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                return JsonSerializer.Deserialize<PagBankBankAccount>(responseContent);
            });
        }

        // Buscar conta bancária
        public async Task<PagBankResponse<PagBankBankAccount>> GetBankAccountAsync(string accountId)
        {
            return await HandleActionAsync(async () =>
            {
                var httpResponse = await _httpClient.GetAsync($"{_baseUrl}/accounts/{accountId}/bank-accounts");
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"PagBank API Error: {responseContent}");
                }
                
                var bankAccounts = JsonSerializer.Deserialize<PagBankBankAccountList>(responseContent);
                return bankAccounts?.Items?.FirstOrDefault();
            });
        }

        // Deletar conta (equivalente ao DeleteAsync do Stripe)
        public async Task<PagBankResponse<bool>> DeleteAccountAsync(string accountId)
        {
            return await HandleActionAsync(async () =>
            {
                var httpResponse = await _httpClient.DeleteAsync($"{_baseUrl}/accounts/{accountId}");
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    throw new Exception($"PagBank API Error: {errorContent}");
                }
                
                return true;
            });
        }
    }

    // Modelos baseados na documentação do PagBank
    public class PagBankResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }

    public class PagBankAccount
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Document { get; set; }
        public string Type { get; set; } // individual ou company
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PagBankAccountRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Document { get; set; }
        public string Type { get; set; } // individual ou company
        public PagBankAddressRequest Address { get; set; }
        public PagBankPhoneRequest Phone { get; set; }
    }

    public class PagBankAddressRequest
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; } = "BR";
    }

    public class PagBankPhoneRequest
    {
        public string Country { get; set; } = "55";
        public string Area { get; set; }
        public string Number { get; set; }
    }

    public class PagBankBankAccount
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string AgencyNumber { get; set; }
        public string HolderName { get; set; }
        public string HolderType { get; set; } // individual ou company
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PagBankBankAccountRequest
    {
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string AgencyNumber { get; set; }
        public string HolderName { get; set; }
        public string HolderType { get; set; } // individual ou company
    }

    public class PagBankBankAccountList
    {
        public List<PagBankBankAccount> Items { get; set; }
    }
}

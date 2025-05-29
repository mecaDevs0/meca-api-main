
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meca.Data.Entities;
using Meca.Data.Enum;
using Meca.Domain.Services.Interface;
using Meca.Domain.ViewModels;
using Microsoft.Extensions.Hosting;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Iugu.Core3.Entity;
using UtilityFramework.Services.Iugu.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3.Models;
using UtilityFramework.Services.Iugu.Core3.Request;

namespace Meca.Domain.Services
{
    public class UtilService : IUtilService
    {
        private readonly IIuguChargeServices _iuguChargeServices;
        private readonly IIuguMarketPlaceServices _iuguMarketPlaceServices;
        private readonly IBusinessBaseAsync<Transfer> _transferRepository;
        private readonly bool _isSandBox;

        public UtilService(
            IIuguChargeServices iuguChargeServices,
            IIuguMarketPlaceServices iuguMarketPlaceServices,
            IBusinessBaseAsync<Transfer> transferRepository,
            IHostingEnvironment env)
        {
            _isSandBox = Util.IsSandBox(env);
            _iuguChargeServices = iuguChargeServices;
            _iuguMarketPlaceServices = iuguMarketPlaceServices;
            _transferRepository = transferRepository;
        }

        public string GetFlag(string flag)
        {
            flag = flag?.ToLower();

            string urlBase = Util.GetCustomUrl(0);

            return string.IsNullOrEmpty(flag) == false ? $"{urlBase}/content/images/flagcard/{flag}.png" : null;
        }


        public async Task<TransactionViewModel> TransactionCreditCard(PayerModel payer, string accountKey, string tokenCard, double price, string description, int installments)
        {
            var response = new TransactionViewModel();

            try
            {
                var checkout = await _iuguChargeServices.TransacaoCreditCardAsync(new IuguChargeRequest()
                {
                    CustomerId = accountKey,
                    Token = tokenCard,
                    DiscountCents = 0,
                    Email = payer.Email,
                    PayerCustomer = payer,
                    Months = installments,
                    InvoiceItems = new[] {
                            new IuguInvoiceItem ( ) {
                                    Description = description,
                                    PriceCents = price.NotAround().ToCent(),
                                    Quantity = 1
                            },
                    }
                }, accountKey, tokenCard);

                if (checkout.HasError)
                {
                    response.Erro = true;
                    response.Error = checkout.Error;
                    response.Message = checkout.GetErrorMessage();
                }
                else
                {
                    response.InvoiceId = checkout.InvoiceId;
                    response.Message = checkout.Message;
                }
            }
            catch (Exception e)
            {
                response.Erro = true;
                response.Message = "Ocorreu um erro ao tentar efetuar o pagamento.";
                response.MessageEx = $"{e.InnerException} {e.Message}".Trim();
            }

            return response;
        }

        public async Task<TransactionViewModel> GenerateBankSlip(PayerModel payer, string accountKey, double price, string description, int dueDateDays = 1, string payableWith = "bank_slip")
        {

            var response = new TransactionViewModel();

            var dueDate = DateTime.Now.AddDays(dueDateDays).ToString("yyyy/MM/dd");
            try
            {
                var bankSlip = await _iuguChargeServices.GerarFaturaAsync(new InvoiceRequestMessage()
                {
                    Payer = payer,
                    CustomerId = accountKey,
                    DueDate = dueDate,
                    EnsureWorkdayDueDate = true,
                    PayableWith = payableWith,
                    Items = new List<Item>() {
                        new Item(){
                            Description = description,
                            PriceCents = price.ToCent(),
                            Quantity = 1
                        }
                    },
                    Email = payer.Email
                });

                if (bankSlip.HasError)
                {
                    response.Erro = true;
                    response.Message = bankSlip.MessageError;
                    response.Error = bankSlip.Error;

                    return response;
                }

                response.InvoiceId = bankSlip.Id;
                response.BankSlipUrl = bankSlip.SecureUrl;
                response.BankSlipBarcode = bankSlip.BankSlip.DigitableLine;
                response.BankSlipPdf = $"{bankSlip.SecureUrl}.pdf";
                response.BankSlipDueDate = Utilities.SetCustomMask(bankSlip.DueDate.OnlyNumbers(), "$3/$2/$1", @"(\d{4})(\d{2})(\d{2})");
                response.UrlSetPaid = $"https://faturas.iugu.com/iugu_bank_slip/{bankSlip.SecureUrl.Split('/').LastOrDefault()}/sample/pay";
                response.PixQrCode = bankSlip.Pix?.Qrcode;
                response.PixQrCodeTxt = bankSlip.Pix?.QrcodeText;
            }
            catch (Exception e)
            {
                response.Erro = true;
                response.Message = "Ocorreu um erro ao tentar efetuar o pagamento.";
                response.MessageEx = $"{e.InnerException} {e.Message}".Trim();
            }
            return response;

        }

        public async Task<bool> Transfer(RecipientType recipientType, string invoiceId, string destinationAccountId, string destinationLiveKey, double value, double sourceValue, double fees, string sourceLiveKey = null)
        {
            try
            {
                IuguTransferModel iuguTransfer = null;

                if (_isSandBox == false)
                    iuguTransfer = await _iuguMarketPlaceServices.RepasseValoresAsync(destinationLiveKey, destinationAccountId, (decimal)value, sourceLiveKey, false);

                var transferEntity = new Transfer()
                {
                    Value = value,
                    SourceValue = sourceValue,
                    Fees = fees,
                    RecipientType = recipientType,
                    AccountId = destinationAccountId,
                    InvoiceId = invoiceId,
                    TransferId = iuguTransfer?.Id
                };

                await _transferRepository.CreateAsync(transferEntity);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}

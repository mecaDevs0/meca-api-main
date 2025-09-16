using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class PagBankOrderRequest
    {
        [Required]
        public string ReferenceId { get; set; }
        
        [Required]
        public PagBankCustomer Customer { get; set; }
        
        [Required]
        public List<PagBankItem> Items { get; set; }
        
        [Required]
        public PagBankShipping Shipping { get; set; }
        
        [Required]
        public List<PagBankPaymentMethod> PaymentMethods { get; set; }
        
        public string NotificationUrls { get; set; }
    }

    public class PagBankCustomer
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string TaxId { get; set; }
        
        public PagBankPhone Phone { get; set; }
    }

    public class PagBankPhone
    {
        [Required]
        public string Country { get; set; } = "55";
        
        [Required]
        public string Area { get; set; }
        
        [Required]
        public string Number { get; set; }
    }

    public class PagBankItem
    {
        [Required]
        public string ReferenceId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public int UnitAmount { get; set; }
    }

    public class PagBankShipping
    {
        [Required]
        public PagBankAddress Address { get; set; }
    }

    public class PagBankAddress
    {
        [Required]
        public string Street { get; set; }
        
        [Required]
        public string Number { get; set; }
        
        public string Complement { get; set; }
        
        [Required]
        public string Locality { get; set; }
        
        [Required]
        public string City { get; set; }
        
        [Required]
        public string RegionCode { get; set; }
        
        [Required]
        public string Country { get; set; } = "BRA";
        
        [Required]
        public string PostalCode { get; set; }
    }

    public class PagBankPaymentMethod
    {
        [Required]
        public string Type { get; set; } // CREDIT_CARD, PIX, BOLETO
        
        public PagBankCreditCard CreditCard { get; set; }
        
        public PagBankPix Pix { get; set; }
        
        public PagBankBoleto Boleto { get; set; }
    }

    public class PagBankCreditCard
    {
        [Required]
        public string Number { get; set; }
        
        [Required]
        public string ExpMonth { get; set; }
        
        [Required]
        public string ExpYear { get; set; }
        
        [Required]
        public string SecurityCode { get; set; }
        
        [Required]
        public PagBankCardHolder Holder { get; set; }
        
        public bool Store { get; set; } = false;
    }

    public class PagBankCardHolder
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        public PagBankPhone Phone { get; set; }
        
        public PagBankAddress Address { get; set; }
    }

    public class PagBankPix
    {
        public int ExpirationDate { get; set; } = 3600; // 1 hora em segundos
    }

    public class PagBankBoleto
    {
        public int DueDate { get; set; } = 86400; // 1 dia em segundos
        
        public PagBankBoletoInstructionLines InstructionLines { get; set; }
    }

    public class PagBankBoletoInstructionLines
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
    }

    public class PagBankOrderResponse
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public PagBankOrderAmount Amount { get; set; }
        public List<PagBankPaymentResponse> PaymentResponses { get; set; }
        public List<PagBankNotificationUrl> NotificationUrls { get; set; }
        public PagBankLinks Links { get; set; }
    }

    public class PagBankOrderAmount
    {
        public int Value { get; set; }
        public string Currency { get; set; } = "BRL";
        public PagBankOrderSummary Summary { get; set; }
    }

    public class PagBankOrderSummary
    {
        public int Total { get; set; }
        public int Paid { get; set; }
        public int Refunded { get; set; }
    }

    public class PagBankPaymentResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string PaymentMethodId { get; set; }
        public PagBankPaymentAmount Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PagBankPaymentMethodResponse PaymentMethod { get; set; }
        public PagBankPaymentLinks Links { get; set; }
    }

    public class PagBankPaymentAmount
    {
        public int Value { get; set; }
        public string Currency { get; set; } = "BRL";
    }

    public class PagBankPaymentMethodResponse
    {
        public string Type { get; set; }
        public PagBankPixResponse Pix { get; set; }
        public PagBankBoletoResponse Boleto { get; set; }
        public PagBankCreditCardResponse CreditCard { get; set; }
    }

    public class PagBankPixResponse
    {
        public DateTime ExpirationDate { get; set; }
        public List<PagBankPixAdditionalInformation> AdditionalInformation { get; set; }
    }

    public class PagBankPixAdditionalInformation
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class PagBankBoletoResponse
    {
        public string Id { get; set; }
        public string Barcode { get; set; }
        public string FormattedBarcode { get; set; }
        public DateTime DueDate { get; set; }
        public PagBankOrderAmount Amount { get; set; }
        public PagBankBoletoInstructionLines InstructionLines { get; set; }
        public PagBankBoletoLinks Links { get; set; }
    }

    public class PagBankBoletoLinks
    {
        public string Pdf { get; set; }
        public string Png { get; set; }
    }

    public class PagBankCreditCardResponse
    {
        public string Brand { get; set; }
        public string FirstDigits { get; set; }
        public string LastDigits { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public PagBankCardHolder Holder { get; set; }
    }

    public class PagBankNotificationUrl
    {
        public string Url { get; set; }
    }

    public class PagBankLinks
    {
        public string Self { get; set; }
        public string Payment { get; set; }
    }

    public class PagBankPaymentLinks
    {
        public string Self { get; set; }
        public string Order { get; set; }
    }

    public class PagBankPaymentRequest
    {
        [Required]
        public string OrderId { get; set; }
        
        [Required]
        public string PaymentMethodId { get; set; }
    }

    public class PagBankOrderDetails
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PagBankOrderAmount Amount { get; set; }
        public List<PagBankPaymentResponse> PaymentResponses { get; set; }
        public PagBankCustomer Customer { get; set; }
        public List<PagBankItem> Items { get; set; }
        public PagBankShipping Shipping { get; set; }
        public List<PagBankNotificationUrl> NotificationUrls { get; set; }
        public PagBankLinks Links { get; set; }
    }

    public class PagBankWebhookResponse
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Event { get; set; }
        public DateTime CreatedAt { get; set; }
        public PagBankWebhookData Data { get; set; }
    }

    public class PagBankWebhookData
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PagBankOrderAmount Amount { get; set; }
        public List<PagBankPaymentResponse> PaymentResponses { get; set; }
    }
}

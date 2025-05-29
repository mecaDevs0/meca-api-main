using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Meca.Domain.ViewModels.Auxiliaries;

namespace Meca.Domain.ViewModels
{
    public class FinancialHistoryViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Cliente
        /// </summary>
        [Display(Name = "Cliente")]
        public ProfileAuxViewModel Profile { get; set; }
        /// <summary>
        /// Oficina
        /// </summary>
        [Display(Name = "Oficina")]
        public WorkshopAuxViewModel Workshop { get; set; }
        /// <summary>
        /// Serviços
        /// </summary>
        [Display(Name = "Serviços")]
        public List<WorkshopServicesAuxViewModel> WorkshopServices { get; set; }
        /// <summary>
        /// Veículo
        /// </summary>
        [Display(Name = "Veículo")]
        public VehicleAuxViewModel Vehicle { get; set; }
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
        /// <summary>
        /// Valor total (Bruto)
        /// </summary>
        [Display(Name = "Valor total (Bruto)")]
        public double Value { get; set; }
        /// <summary>
        /// Valor Liquido
        /// </summary>
        [Display(Name = "Valor Liquido")]
        public double NetValue { get; set; }
        /// <summary>
        /// Taxas de processamento
        /// </summary>
        [Display(Name = "Taxas de processamento")]
        public double? ProcessingValue { get; set; }
        /// <summary>
        /// Data do pagamento
        /// </summary>
        [Display(Name = "Data do pagamento")]
        public long? PaymentDate { get; set; }
        /// <summary>
        /// Data da liberação do valor
        /// </summary>
        [Display(Name = "Data da liberação do valor")]
        public long? ReleasedDate { get; set; }
        /// <summary>
        /// Data de expiração da cobrança
        /// </summary>
        [Display(Name = "Data de expiração da cobrança")]
        public long? ExpiredDate { get; set; }
        /// <summary>
        /// Data do estorno
        /// </summary>
        [Display(Name = "Data do estorno")]
        public long? RefundDate { get; set; }
        /// <summary>
        /// Valor estornado
        /// </summary>
        [Display(Name = "Valor estornado")]
        public double? ReversedValue { get; set; }
        /// <summary>
        /// Taxa da plataforma (porcentagem) 
        /// </summary>
        [Display(Name = "Taxa da plataforma (porcentagem)")]
        public double? PlatformFee { get; set; }
        /// <summary>
        /// Valor líquido mecânico
        /// </summary>
        [Display(Name = "Valor líquido mecânico")]
        public double? MechanicalNetValue { get; set; }
        /// <summary>
        /// Valor da plataforma
        /// </summary>
        [Display(Name = "Valor da plataforma")]
        public double? PlatformValue { get; set; }
        /// <summary>
        /// Quantidade de parcelas
        /// </summary>
        [Display(Name = "Quantidade de parcelas")]
        public int Installment { get; set; } = 1;
        /// <summary>
        /// Identificador do cartão de crédito
        /// </summary>
        [Display(Name = "Identificador do cartão de crédito")]
        public string CreditCardId { get; set; }
        /// <summary>
        /// Status do pagamento
        /// </summary>
        [Display(Name = "Status do pagamento")]
        public PaymentStatus PaymentStatus { get; set; }
        /// <summary>
        /// Forma de pagamento
        /// </summary>
        [Display(Name = "Forma de pagamento")]
        public PaymentMethod PaymentMethod { get; set; }
        /// <summary>
        /// ID da transação
        /// </summary>
        [Display(Name = "ID da transação")]
        public string InvoiceId { get; set; }

        /* TRANSAÇÃO */

        /// <summary>
        /// Qr code (Imagem)
        /// </summary>
        [Display(Name = "Qr code")]
        public string PixQrCode { get; set; }
        /// <summary>
        /// Conteudo do qrcode
        /// </summary>
        [Display(Name = "Conteudo do qrcode")]
        public string PixQrCodeTxt { get; set; }
    }
}
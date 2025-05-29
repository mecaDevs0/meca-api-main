using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Export
{
    public class FinancialHistoryExportViewModel
    {
        // Informações do usuário

        /// <summary>
        /// Nome do usuário
        /// </summary>
        [Display(Name = "Nome do usuário")]
        public string ProfileName { get; set; }
        /// <summary>
        /// Placa do carro
        /// </summary>
        [Display(Name = "Placa do carro")]
        public string VehiclePlate { get; set; }
        /// <summary>
        /// Serviços solicitados
        /// </summary>
        [Display(Name = "Serviços solicitados")]
        public string Services { get; set; }

        // Informações da oficina

        /// <summary>
        /// Nome da empresa
        /// </summary>
        [Display(Name = "Nome da empresa")]
        public string WorkshopName { get; set; }
        /// <summary>
        /// CNPJ
        /// </summary>
        [Display(Name = "CNPJ")]
        public string WorkshopCnpj { get; set; }
        /// <summary>
        /// Nome do responsável
        /// </summary>
        [Display(Name = "Nome do responsável")]
        public string WorkshopResponsibleName { get; set; }

        // Geral

        /// <summary>
        /// Status
        /// </summary>
        [Display(Name = "Status")]
        public string Status { get; set; }
        /// <summary>
        /// Data da última atualização
        /// </summary>
        [Display(Name = "Data da última atualização")]
        public string LastUpdate { get; set; }
        /// <summary>
        /// Valor pago
        /// </summary>
        [Display(Name = "Valor pago")]
        public string Value { get; set; }
        /// <summary>
        /// Data de pagamento
        /// </summary>
        [Display(Name = "Data de pagamento")]
        public string PaymentDate { get; set; }
        /// <summary>
        /// Forma de pagamento
        /// </summary>
        [Display(Name = "Forma de pagamento")]
        public string PaymentMethod { get; set; }
        /// <summary>
        /// Data do estorno
        /// </summary>
        [Display(Name = "Data do estorno")]
        public string RefundDate { get; set; }
        /// <summary>
        /// Valor estornado
        /// </summary>
        [Display(Name = "Valor estornado")]
        public string ReversedValue { get; set; }
        /// <summary>
        /// Valor pago para a oficina
        /// </summary>
        [Display(Name = "Valor pago para a oficina")]
        public string AmountPaidForTheWorkshop { get; set; }
        /// <summary>
        /// Valor pago para a Meca
        /// </summary>
        [Display(Name = "Valor pago para a Meca")]
        public string AmountPaidToMecca { get; set; }
        /// <summary>
        /// Taxa aplicada
        /// </summary>
        [Display(Name = "Taxa aplicada")]
        public string ProcessingValue { get; set; }
    }
}
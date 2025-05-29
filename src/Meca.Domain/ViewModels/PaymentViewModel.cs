using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Meca.Domain;

namespace Meca.Domain.ViewModels
{
    public class PaymentViewModel
    {
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Display(Name = "ID do agendamento")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string SchedulingId { get; set; }
        /// <summary>
        /// Identificador do pagamento no gateway
        /// </summary>
        [Display(Name = "Identificador do pagamento no gateway")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string InvoiceId { get; set; }
    }
}
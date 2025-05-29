using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;

namespace Meca.Domain.ViewModels.Filters
{
    public class FinancialHistoryFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Identificador do usuário
        /// </summary>
        [Display(Name = "Identificador do usuário")]
        public string ProfileId { get; set; }
        /// <summary>
        /// Identificador da oficina
        /// </summary>
        [Display(Name = "Identificador da oficina")]
        public string WorkshopId { get; set; }
        /// <summary>
        /// Início do filtro
        /// </summary>
        [Display(Name = "Início do filtro")]
        public long? StartDate { get; set; }
        /// <summary>
        /// Fim do filtro
        /// </summary>
        [Display(Name = "Fim do filtro")]
        public long? EndDate { get; set; }
        /// <summary>
        /// Status de pagamento
        /// </summary>
        [Display(Name = "Status de pagamento")]
        public List<PaymentStatus> PaymentStatus { get; set; } = [];
        /// <summary>
        /// Identificadores das transações
        /// </summary>
        [Display(Name = "Identificadores das transações")]
        public List<string> Id { get; set; } = [];
    }
}
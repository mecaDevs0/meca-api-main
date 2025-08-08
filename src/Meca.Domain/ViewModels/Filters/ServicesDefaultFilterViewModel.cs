using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Filters
{
    public class ServicesDefaultFilterViewModel : BaseFilterViewModel
    {
        /// <summary>
        /// Nome do serviço
        /// </summary>
        [Display(Name = "Nome do serviço")]
        public string Name { get; set; }
        /// <summary>
        /// Tipos de serviço
        /// </summary>
        [Display(Name = "Tipos de serviço")]
        public List<string> ServiceTypes { get; set; }

        // Task para remover preço do serviço
        // /// <summary>
        // /// Faixa de preço (Inicial)
        // /// </summary>
        // [Display(Name = "Faixa de preço (Inicial)")]
        // public double? PriceRangeInitial { get; set; }
        // /// <summary>
        // /// Faixa de preço (Final)
        // /// </summary>
        // [Display(Name = "Faixa de preço (Final)")]
        // public double? PriceRangeFinal { get; set; }

        /// <summary>
        /// Avaliação (estrelas)
        /// </summary>
        [Display(Name = "Avaliação (estrelas)")]
        [Range(0, 5, ErrorMessage = "A avaliação deve estar entre 0 e 5.")]
        public int? Rating { get; set; }
        /// <summary>
        /// Distância (KM) 
        /// </summary>
        [Display(Name = "Distância (KM)")]
        public int? Distance { get; set; }
        /// <summary>
        /// Latitude (usuário) 
        /// </summary>
        [Display(Name = "Latitude (usuário)")]
        public string LatUser { get; set; }
        /// <summary>
        /// Longitude (usuário) 
        /// </summary>
        [Display(Name = "Longitude (usuário)")]
        public string LongUser { get; set; }
        /// <summary>
        /// ID do usuário
        /// </summary>
        [Display(Name = "ID do usuário")]
        public string? ProfileId { get; set; }
    }
}
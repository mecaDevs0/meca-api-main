using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Domain.ViewModels.Auxiliaries
{
    [BsonIgnoreExtraElements]
    public class BudgetServicesAuxViewModel
    {
        /// <summary>
        /// Titulo do serviço
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Titulo do serviço")]
        public string Title { get; set; }
        /// <summary>
        /// Descrição do serviço
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Description { get; set; }
        /// <summary>
        /// Valor do serviço
        /// </summary>
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public double Value { get; set; }
    }
}
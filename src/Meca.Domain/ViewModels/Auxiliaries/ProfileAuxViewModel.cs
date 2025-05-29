using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Auxiliaries
{
    public class ProfileAuxViewModel
    {
        /// <summary>
        /// Identificador
        /// </summary>
        [Display(Name = "Identificador")]
        public string Id { get; set; }
        /// <summary>
        /// Nome completo
        /// </summary>
        [Display(Name = "Nome completo")]
        public string FullName { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        [Display(Name = "Email")]
        public string Email { get; set; }
        /// <summary>
        /// Telefone
        /// </summary>
        [Display(Name = "Telefone")]
        public string Phone { get; set; }
    }
}
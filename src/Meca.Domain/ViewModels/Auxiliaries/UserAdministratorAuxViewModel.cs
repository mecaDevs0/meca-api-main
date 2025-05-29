using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels.Auxiliaries
{
    public class UserAdministratorAuxViewModel
    {
        /// <summary>
        /// Identificador
        /// </summary>
        [Display(Name = "Identificador")]
        public string Id { get; set; }
        /// <summary>
        /// Nome
        /// </summary>
        [Display(Name = "Nome")]
        public string Name { get; set; }
        /// <summary>
        /// E-mail
        /// </summary>
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}
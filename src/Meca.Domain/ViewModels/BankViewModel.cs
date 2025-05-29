using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels
{
    public class BankViewModel : BaseViewModel
    {
        /// <summary>
        /// Código do banco
        /// </summary>
        [Display(Name = "Código do banco")]
        public string Code { get; set; }
        /// <summary>
        /// Nome
        /// </summary>
        [Display(Name = "Nome")]
        public string Name { get; set; }
    }
}
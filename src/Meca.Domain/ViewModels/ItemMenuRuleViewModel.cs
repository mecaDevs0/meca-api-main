using System.ComponentModel.DataAnnotations;

using Meca.Data.Enum;

using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels
{
    public class ItemMenuRuleViewModel
    {
        /// <summary>
        /// Item do Menu
        /// </summary>
        [Display(Name = "Item do Menu")]
        public MenuItem MenuItem { get; set; }
        /// <summary>
        /// Submenu
        /// </summary>
        [Display(Name = "Submenu")]
        public SubMenuItem? SubMenu { get; set; }
        /// <summary>
        /// Pode visualizar
        /// </summary>
        [Display(Name = "Pode visualizar")]
        public bool Access { get; set; } = true;
        /// <summary>
        /// Pode editar
        /// </summary>
        [Display(Name = "Pode editar")]
        public bool Edit { get; set; } = true;
        /// <summary>
        /// Pode registrar
        /// </summary>
        [Display(Name = "Pode registrar")]
        public bool Write { get; set; } = true;
        /// <summary>
        /// Pode remover
        /// </summary>
        [Display(Name = "Pode remover")]
        public bool Delete { get; set; } = true;
        /// <summary>
        /// Pode exportar
        /// </summary>
        [Display(Name = "Pode exportar")]
        public bool Export { get; set; }
        /// <summary>
        /// Ativar/Desativar
        /// </summary>
        [Display(Name = "Ativar/Desativar")]
        public bool? EnableDisable { get; set; }

    }
}
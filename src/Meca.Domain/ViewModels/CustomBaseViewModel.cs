using System;
using System.ComponentModel.DataAnnotations;

using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels
{
    public class CustomBaseViewModel : BaseViewModel
    {
        /// <summary>
        /// Ativo/Inativo (unix seconds = Inátivo | null = ativo)
        /// </summary>
        [Display(Name = "Ativo/Inativo")]
        public long? DataBlocked { get; set; }
        /// <summary>
        /// Removido logicamente
        /// </summary>
        [Display(Name = "Removido logicamente")]
        [IsReadOnly]
        public long? Disabled { get; set; }
        /// <summary>
        /// Data de cadastro
        /// </summary>
        [Display(Name = "Data de cadastro")]
        [IsReadOnly]
        public long? Created { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}
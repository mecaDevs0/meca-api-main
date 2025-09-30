using System;
using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels
{
    public class AvailableTimeViewModel : BaseViewModel
    {
        /// <summary>
        /// Horário disponível
        /// </summary>
        [Display(Name = "Horário")]
        public DateTime Time { get; set; }

        /// <summary>
        /// Se o horário está disponível
        /// </summary>
        [Display(Name = "Disponível")]
        public bool Available { get; set; }

        /// <summary>
        /// Horário formatado (HH:mm)
        /// </summary>
        [Display(Name = "Horário Formatado")]
        public string FormattedTime { get; set; }
    }
}


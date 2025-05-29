using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class WorkshopAgendaAuxViewModel
    {
        /// <summary>
        /// Aberto?
        /// </summary>
        [Display(Name = "Aberto?")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public bool Open { get; set; }
        /// <summary>
        /// Horário de início
        /// </summary>
        [Display(Name = "Horário de início")]
        [RequiredIf("Open", true, ErrorMessage = DefaultMessages.FieldRequired)]
        public string StartTime { get; set; }
        /// <summary>
        /// Horário de fechamento
        /// </summary>
        [Display(Name = "Horário de fechamento")]
        [RequiredIf("Open", true, ErrorMessage = DefaultMessages.FieldRequired)]
        public string ClosingTime { get; set; }
        /// <summary>
        /// Horário de início do intervalo
        /// </summary>
        [Display(Name = "Horário de início do intervalo")]
        [RequiredIf("Open", true, ErrorMessage = DefaultMessages.FieldRequired)]
        public string StartOfBreak { get; set; }
        /// <summary>
        /// Horário de término do intervalo
        /// </summary>
        [Display(Name = "Horário de término do intervalo")]
        [RequiredIf("Open", true, ErrorMessage = DefaultMessages.FieldRequired)]
        public string EndOfBreak { get; set; }
    }
}
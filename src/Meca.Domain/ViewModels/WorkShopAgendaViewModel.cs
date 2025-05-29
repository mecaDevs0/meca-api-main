using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class WorkshopAgendaViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Domingo
        /// </summary>
        [Display(Name = "Domingo")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public WorkshopAgendaAuxViewModel Sunday { get; set; }
        /// <summary>
        /// Segunda-feira
        /// </summary>
        [Display(Name = "Segunda-feira")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public WorkshopAgendaAuxViewModel Monday { get; set; }
        /// <summary>
        /// Terça-feira
        /// </summary>
        [Display(Name = "Terça-feira")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public WorkshopAgendaAuxViewModel Tuesday { get; set; }
        /// <summary>
        /// Quarta-feira
        /// </summary>
        [Display(Name = "Quarta-feira")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public WorkshopAgendaAuxViewModel Wednesday { get; set; }
        /// <summary>
        /// Quinta-feira
        /// </summary>
        [Display(Name = "Quinta-feira")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public WorkshopAgendaAuxViewModel Thursday { get; set; }
        /// <summary>
        /// Sexta-feira
        /// </summary>
        [Display(Name = "Sexta-feira")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public WorkshopAgendaAuxViewModel Friday { get; set; }
        /// <summary>
        /// Sábado
        /// </summary>
        [Display(Name = "Sábado")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public WorkshopAgendaAuxViewModel Saturday { get; set; }
        /// <summary>
        /// Oficina
        /// </summary>
        [Display(Name = "Oficina")]
        public WorkshopAuxViewModel Workshop { get; set; }
    }
}
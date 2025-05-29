using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class CalendarAvailableViewModel
    {
        /// <summary>
        /// Data
        /// </summary>
        [Display(Name = "Data")]
        public DateTime Date { get; set; }
        /// <summary>
        /// Disponível?
        /// </summary>
        [Display(Name = "Disponível?")]
        public bool Available { get; set; }
        /// <summary>
        /// Dia da semana
        /// </summary>
        [Display(Name = "Dia da semana")]
        public DayOfWeek DayOfWeek { get; set; }
        /// <summary>
        /// Horas disponíveis (Para o usuário)
        /// </summary>
        [Display(Name = "Horas disponíveis (Para o usuário)")]
        public List<string> Hours { get; set; } = new List<string>();
        /// <summary>
        /// Agenda da oficina
        /// </summary>
        [Display(Name = "Agenda da oficina")]
        public List<WorkshopAgendaHoursViewModel> WorkshopAgenda { get; set; } = [];
    }
}
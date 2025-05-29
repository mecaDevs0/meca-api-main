using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Auxiliaries;

namespace Meca.Domain.ViewModels
{
    public class NotificationViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Título
        /// </summary>
        [Display(Name = "Título")]
        public string Title { get; set; }
        /// <summary>
        /// Mensagem
        /// </summary>
        [Display(Name = "Mensagem")]
        public string Content { get; set; }
        /// <summary>
        /// Data da leitura
        /// </summary>
        [Display(Name = "Data da leitura")]
        public long? DateRead { get; set; }

        // Custom

        /// <summary>
        /// Oficina
        /// </summary>
        [Display(Name = "Oficina")]
        public WorkshopAuxViewModel Workshop { get; set; }
        /// <summary>
        /// Usuário
        /// </summary>
        [Display(Name = "Usuário")]
        public ProfileAuxViewModel Profile { get; set; }
        /// <summary>
        /// ID do agendamento
        /// </summary>
        [Display(Name = "ID do agendamento")]
        public string SchedulingId { get; set; }
    }
}
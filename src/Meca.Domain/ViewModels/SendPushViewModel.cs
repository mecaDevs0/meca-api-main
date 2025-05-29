using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;

namespace Meca.Domain.ViewModels
{
    public class SendPushViewModel
    {
        public SendPushViewModel()
        {
            Route = RouteNotification.System;
        }
        /// <summary>
        /// Título
        /// </summary>
        [Display(Name = "Título")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Title { get; set; }

        /// <summary>
        /// Mensagem
        /// </summary>
        [Display(Name = "Mensagem")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Content { get; set; }
        /// <summary>
        /// Destinatários (caso length 0 envia para todos)
        /// </summary>
        [Display(Name = "Destinatários")]
        public List<string> TargetId { get; set; } = new List<string>();
        /// <summary>
        /// Tipo de Destinatário
        /// </summary>
        [Display(Name = "Tipo de Destinatários")]
        public TypeProfile TypeProfile { get; set; }
        /// <summary>
        /// Roteamento do push
        /// </summary>
        [Display(Name = "Roteamento do push")]
        public RouteNotification? Route { get; set; }
    }
}
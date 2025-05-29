using System.ComponentModel.DataAnnotations;
using Meca.Domain.ViewModels.Auxiliaries;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.Domain.ViewModels.Admin
{
    public class UserAdministratorViewModel : CustomBaseViewModel
    {
        /// <summary>
        /// Nome
        /// </summary>
        [Display(Name = "Nome")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Name { get; set; }
        /// <summary>
        /// Senha
        /// </summary>
        [Display(Name = "Senha")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }
        /// <summary>
        /// E-mail
        /// </summary>
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        [JsonConverter(typeof(ToLowerCase))]
        public string Email { get; set; }
        /// <summary>
        /// Bloqueado
        /// </summary>
        [Display(Name = "Bloqueado")]
        public bool Blocked { get; set; }
        /// <summary>
        /// Padrão do sistema
        /// </summary>
        [Display(Name = "Padrão do sistema")]
        [IsReadOnly]
        public bool IsDefault { get; set; }
        /// <summary>
        /// Nível de acesso
        /// </summary>
        [Display(Name = "Nível de acesso")]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [IsReadOnly]
        public BaseReferenceAuxViewModel AccessLevel { get; set; }
        /// <summary>
        /// Total de notifações não lidas
        /// </summary>
        [Display(Name = "Total de notifações não lidas")]
        [IsReadOnly]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalNotificationNoRead { get; set; }
    }
}
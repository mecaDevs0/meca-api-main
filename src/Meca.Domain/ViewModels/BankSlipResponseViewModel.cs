using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;

namespace Meca.Domain.ViewModels
{
    public class BankSlipResponseViewModel
    {
        /// <summary>
        /// Identificador
        /// </summary>
        [Display(Name = "Identificador")]
        [IsReadOnly]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        /// <summary>
        /// Código de Barras
        /// </summary>
        [Display(Name = "Código de Barras")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BankSlipBarcode { get; set; }
        /// <summary>
        /// Imagem código de barras
        /// </summary>
        [Display(Name = "Imagem código de barras")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BankSlipBarcodeUrl { get; set; }
        /// <summary>
        /// URL boleto
        /// </summary>
        [Display(Name = "URL boleto")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BankSlipUrl { get; set; }
        /// <summary>
        /// URL pdf
        /// </summary>
        [Display(Name = "URL pdf")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BankSlipPdf { get; set; }
        /// <summary>
        /// Vencimento do boleto
        /// </summary>
        [Display(Name = "Vencimento do boleto")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BankSlipDueDate { get; set; }
        /// <summary>
        /// Qr code (Imagem)
        /// </summary>
        [Display(Name = "Qr code")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PixQrCode { get; set; }
        /// <summary>
        /// Conteudo do qrcode
        /// </summary>
        [Display(Name = "Conteudo do qrcode")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PixQrCodeTxt { get; set; }
        /// <summary>
        /// URL para definir como pago
        /// </summary>
        [Display(Name = "URL para definir como pago")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UrlSetPaid { get; set; }
        /// <summary>
        /// Valor pago
        /// </summary>
        [Display(Name = "Valor pago")]
        [IsReadOnly]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double Value { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using Meca.Data.Enum;

namespace Meca.Domain.ViewModels.Filters
{
    public class BaseFilterViewModel
    {
        /// <summary>
        /// Página (obrigatório caso uso de filtro) de 1 a N
        /// </summary>
        [Display(Name = "Página")]
        public int? Page { get; set; }
        /// <summary>
        /// Limite  (limit = 0 para listar todos | valor default 30)
        /// </summary>
        [Display(Name = "Limite")]
        public int? Limit { get; set; }

        /// <summary>
        /// Filtro de situação (Ativo / Inativo)
        /// </summary>
        [Display(Name = "Filtro de situação")]
        public FilterActived? DataBlocked { get; set; }

        public int SkipDocuments() => (Page.GetValueOrDefault() - 1) * Limit.GetValueOrDefault();

        public void SetDefault(int limit = 30)
        {

            Page = Math.Max(1, Page.GetValueOrDefault()); /*VALOR MÍNIMO 1*/

            /*VALOR DEFAULT*/
            if (Limit == null)
                Limit = limit;

        }
    }
}
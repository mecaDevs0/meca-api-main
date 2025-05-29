using System.ComponentModel.DataAnnotations;

namespace Meca.Domain.ViewModels
{
    public class DashboardViewModel
    {
        /// <summary>
        /// Total de Clientes
        /// </summary>
        [Display(Name = "Total de Clientes")]
        public long TotalClients { get; set; }
        /// <summary>
        /// Total de Oficinas
        /// </summary>
        [Display(Name = "Total de Oficinas")]
        public long TotalWorkshops { get; set; }
        /// <summary>
        /// Total de Serviços
        /// </summary>
        [Display(Name = "Total de Serviços")]
        public long TotalServices { get; set; }
        /// <summary>
        /// Total de Serviços agendados
        /// </summary>
        [Display(Name = "Total de Serviços agendados")]
        public long TotalServicesScheduled { get; set; }
        /// <summary>
        /// Total de Serviços Em aberto
        /// </summary>
        [Display(Name = "Total de Serviços Em aberto")]
        public long TotalOpenServices { get; set; }
        /// <summary>
        /// Total de Serviços finalizados
        /// </summary>
        [Display(Name = "Total de Serviços finalizados")]
        public long TotalServicesCompleted { get; set; }
        /// <summary>
        /// Total de avaliações
        /// </summary>
        [Display(Name = "Total de avaliações")]
        public long TotalRatings { get; set; }
        /// <summary>
        /// Média de avaliações (por estrelas)
        /// </summary>
        [Display(Name = "Média de avaliações (por estrelas)")]
        public double AverageRatings { get; set; }
    }
}
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;
using Meca.ApplicationService.Interface;
using Meca.ApplicationService.Services.Hangfire.Interface;
using Meca.Data.Entities;
using Meca.Data.Enum;
using Meca.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Iugu.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3.Models;


namespace Meca.ApplicationService.Services.Hangfire
{
    public class HangfireService : IHangfireService
    {
        private readonly IBusinessBaseAsync<WithDraw> _withDrawRepository;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;
        private readonly IBusinessBaseAsync<Vehicle> _vehicleRepository;
        private readonly IBusinessBaseAsync<AgendaAux> _agendaAuxRepository;
        private readonly IIuguMarketPlaceServices _iuguMarketPlaceServices;
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _isSandbox;

        public HangfireService(
            IBusinessBaseAsync<WithDraw> withDrawRepository,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IBusinessBaseAsync<Vehicle> vehicleRepository,
            IBusinessBaseAsync<AgendaAux> agendaAuxRepository,
            IIuguMarketPlaceServices iuguMarketPlaceServices,
            IServiceProvider serviceProvider,
            IHostingEnvironment env)
        {
            _withDrawRepository = withDrawRepository;
            _schedulingRepository = schedulingRepository;
            _vehicleRepository = vehicleRepository;
            _agendaAuxRepository = agendaAuxRepository;
            _iuguMarketPlaceServices = iuguMarketPlaceServices;
            _serviceProvider = serviceProvider;
            _isSandbox = Util.IsSandBox(env);
        }

        public async Task NotifyScheduling(PerformContext context = null)
        {
            // Notificar a Oficina e o Usuário sobre o agendamento 30 minutos antes do horário

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var schedulingService = scope.ServiceProvider.GetRequiredService<ISchedulingService>();

                    // Define o fuso horário GMT-3
                    TimeZoneInfo gmtMinus3 = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                    DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtMinus3);

                    // Adiciona 30 minutos à data atual
                    DateTime nowPlus30Minutes = now.AddMinutes(30);

                    // Define os segundos como 00
                    DateTime finalDateTime = new DateTime(
                        nowPlus30Minutes.Year,
                        nowPlus30Minutes.Month,
                        nowPlus30Minutes.Day,
                        nowPlus30Minutes.Hour,
                        nowPlus30Minutes.Minute,
                        0
                    );

                    var schedulingList = (await _schedulingRepository
                        .FindByAsync(x => x.Date >= now.ToUnix() && x.Date == finalDateTime.ToUnix()))
                        .ToList();

                    if (schedulingList.Any())
                    {
                        foreach (var scheduling in schedulingList)
                        {
                            var vehicleEntity = await _vehicleRepository.FindByIdAsync(scheduling.Vehicle.Id);

                            string titleProfile = "Agendamento próximo";
                            StringBuilder messageProfile = new StringBuilder();
                            messageProfile.AppendLine($"Seu agendamento às {scheduling.Date.MapUnixTime("HH:mm", "-")} está confirmado! Não deixe seu meca esperando!<br/>");
                            await schedulingService.SendProfileNotification(titleProfile, messageProfile, scheduling.Profile.Id, scheduling.Workshop);

                            string titleWorkshop = "Agendamento próximo";
                            StringBuilder messageWorkshop = new StringBuilder();
                            messageWorkshop.AppendLine($"Seu agendamento às {scheduling.Date.MapUnixTime("HH:mm", "-")} de n° {scheduling.GetStringId()} chega em 30 minutos!<br/>");
                            messageWorkshop.AppendLine("<p>Informações do veículo abaixo<br/>");
                            messageWorkshop.AppendLine($"<p>Placa: {vehicleEntity.Plate}<br/>");
                            messageWorkshop.AppendLine($"<p>Fabricante: {vehicleEntity.Manufacturer}<br/>");
                            messageWorkshop.AppendLine($"<p>Modelo: {vehicleEntity.Model}<br/>");
                            messageWorkshop.AppendLine($"<p>Quilometragem: {vehicleEntity.Km}<br/>");
                            messageWorkshop.AppendLine($"<p>Cor: {vehicleEntity.Color}<br/>");
                            messageWorkshop.AppendLine($"<p>Ano: {vehicleEntity.Year}<br/>");
                            messageWorkshop.AppendLine($"<p>Data da última revisão: {vehicleEntity.LastRevisionDate.MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/>");
                            await schedulingService.SendWorkshopNotification(titleWorkshop, messageWorkshop, scheduling.Workshop.Id, scheduling.Profile);
                        }
                    }
                }

                context?.WriteLine($"Notificação enviada.");
            }
            catch (Exception ex)
            {
                context?.WriteLine($"Ocorreu um erro: {ex.InnerException} {ex.Message}".Trim());
            }
        }

        public async Task RemoveAgenda(PerformContext context = null)
        {
            // Remover hórarios antigos, removidos da agenda 

            try
            {
                // Define o fuso horário GMT-3
                TimeZoneInfo gmtMinus3 = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtMinus3);

                await _agendaAuxRepository.DeleteAsync(x => x.Date < now.ToUnix());
            }
            catch (Exception ex)
            {
                context?.WriteLine($"Ocorreu um erro: {ex.InnerException} {ex.Message}".Trim());
            }
        }

        private async Task WithDrawRegister(PerformContext context, RecipientType recipientType, string professionalName, string destinationAccountId, string destinationLiveKey, double value)
        {
            try
            {
                IuguWithdrawalModel iuguWithDraw = null;

                if (_isSandbox == false)
                    iuguWithDraw = await _iuguMarketPlaceServices.SolicitarSaqueAsync(destinationAccountId, (decimal)value, destinationLiveKey);

                var withDrawEntity = new WithDraw()
                {
                    WithDrawId = iuguWithDraw?.WithdrawalId,
                    AccountId = destinationAccountId,
                    Value = value,
                    RecipientType = recipientType
                };

                await _withDrawRepository.CreateAsync(withDrawEntity);

                context?.WriteLine($"Solicitado saque de {value:C} para subconta {professionalName}");
            }
            catch (Exception ex)
            {
                context?.WriteLine($"Ocorreu um erro: {ex.InnerException} {ex.Message}".Trim());
            }
        }
    }


}
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class PaymentNotificationHub : Hub
{
    public async Task SendPaymentNotification(string invoiceId)
    {
        // Envia notificação para todos os clientes conectados
        await Clients.All.SendAsync("PaymentConfirmed", invoiceId);
    }
}

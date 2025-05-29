using UtilityFramework.Application.Core3;


namespace Meca.Domain.ViewModels
{
    public class TransactionViewModel : BankSlipResponseViewModel
    {
        [IsReadOnly]
        public bool Erro { get; set; }
        [IsReadOnly]
        public object Error { get; set; }
        [IsReadOnly]
        public string Message { get; set; }
        [IsReadOnly]
        public string MessageEx { get; set; }
        public string InvoiceId { get; set; }
    }
}
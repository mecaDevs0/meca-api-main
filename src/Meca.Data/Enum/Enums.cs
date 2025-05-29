using System.ComponentModel;
using System.Runtime.Serialization;

namespace Meca.Data.Enum
{
    public enum RouteNotification
    {
        [EnumMember(Value = "Mensagem de sistema")]
        [Description("Mensagem de sistema")]
        System = 0
    }

    public enum IndexPush
    {
        Profile = 0,
        Workshop = 1
    }

    public enum PaymentStatus
    {
        [EnumMember(Value = "Pendente")]
        [Description("Pendente")]
        Pending = 0,
        [EnumMember(Value = "Pago")]
        [Description("Pago")]
        Paid = 1,
        [EnumMember(Value = "Recusado")]
        [Description("Recusado")]
        Declined = 2,
        [EnumMember(Value = "Vencido")]
        [Description("Vencido")]
        OverDue = 3,
        [EnumMember(Value = "Cancelado")]
        [Description("Cancelado")]
        Canceled = 4,
        [EnumMember(Value = "Liberado")]
        [Description("Liberado")]
        Released = 5,
        [EnumMember(Value = "Estornado")]
        [Description("Estornado")]
        Refund = 6
    }

    public enum PaymentMethod
    {
        [EnumMember(Value = "Cartão de crédito")]
        [Description("Cartão de crédito")]
        CreditCard = 0,
        [EnumMember(Value = "Pix")]
        [Description("Pix")]
        Pix = 1,
        [EnumMember(Value = "Apple Pay")]
        [Description("Apple Pay")]
        ApplePay = 2,
        [EnumMember(Value = "Google Pay")]
        [Description("Google Pay")]
        GooglePay = 3,
    }

    public enum SortByCustom
    {
        [EnumMember(Value = "Crescente")]
        [Description("Crescente")]
        Asc = 0,
        [EnumMember(Value = "Decrescente")]
        [Description("Decrescente")]
        Desc = 1
    }

    public enum FilterActived
    {
        [EnumMember(Value = "Ativo")]
        [Description("Ativo")]
        Actived = 0,
        [EnumMember(Value = "Inativo")]
        [Description("Inativo")]
        Disabled = 1
    }

    public enum RecipientType
    {
        [EnumMember(Value = "Plataforma")]
        [Description("Plataforma")]
        Platform = 0,
        [EnumMember(Value = "Carteira do App")]
        [Description("Carteira do App")]
        Wallet = 1,
        [EnumMember(Value = "Taxas megaleios")]
        [Description("Taxas megaleios")]
        Mega = 2
    }

    public enum TypeProvider
    {
        [EnumMember(Value = "Senha")]
        [Description("Senha")]
        Password = 0,
        [EnumMember(Value = "Facebook")]
        [Description("Facebook")]
        Facebook = 1,
        [EnumMember(Value = "AppleId")]
        [Description("AppleId")]
        Apple = 2,
        [EnumMember(Value = "GoogleId")]
        [Description("GoogleId")]
        Google = 3
    }

    public enum TypeAccount
    {
        [EnumMember(Value = "Corrente")]
        [Description("Corrente")]
        CC = 0,
        [EnumMember(Value = "Poupança")]
        [Description("Poupança")]
        CP = 1

    }

    public enum TypePersonBank
    {
        [EnumMember(Value = "Pessoa Jurídica")]
        [Description("Pessoa Jurídica")]
        LegalPerson = 0,
        [EnumMember(Value = "Pessoa Física")]
        [Description("Pessoa Física")]
        PhysicalPerson = 1
    }

    public enum Language
    {
        [EnumMember(Value = "Ingles")]
        [Description("Ingles")]
        En = 0,
        [EnumMember(Value = "Espanhol")]
        [Description("Espanhol")]
        Es = 1,
        [EnumMember(Value = "Português")]
        [Description("Português")]
        Pt = 2

    }

    public enum TypeProfile
    {
        [EnumMember(Value = "Usuário administrador")]
        [Description("Usuário administrador")]
        UserAdministrator = 0,
        [EnumMember(Value = "Cliente")]
        [Description("Cliente")]
        Profile = 1,
        [EnumMember(Value = "Oficina")]
        [Description("Oficina")]
        Workshop = 2
    }

    public enum DataBankStatus
    {
        [EnumMember(Value = "Não informado")]
        [Description("Não informado")]
        Uninformed = 0,
        [EnumMember(Value = "Verificando")]
        [Description("Verificando")]
        Checking = 1,
        [EnumMember(Value = "Válido")]
        [Description("Válido")]
        Valid = 2,
        [EnumMember(Value = "Inválido")]
        [Description("Inválido")]
        Invalid = 3
    }

    public enum WorkshopStatus
    {
        [EnumMember(Value = "Aguardando aprovação")]
        [Description("Aguardando aprovação")]
        AwaitingApproval = 0,
        [EnumMember(Value = "Aprovado")]
        [Description("Aprovado")]
        Approved = 1,
        [EnumMember(Value = "Reprovado")]
        [Description("Reprovado")]
        Disapprove = 2
    }

    public enum SchedulingStatus
    {
        [EnumMember(Value = "Aguardando agendamento")]
        [Description("Aguardando confirmação da Oficina")]
        WaitingAppointment = 0,
        [EnumMember(Value = "Horário sugerido pela Oficina")]
        [Description("Novo horário sugerido pela Oficina")]
        SuggestedTime = 1,
        [EnumMember(Value = "Agendamento recusado")]
        [Description("Agendamento recusado")]
        AppointmentRefused = 2,
        [EnumMember(Value = "Agendamento confirmado")]
        [Description("Agendamento confirmado")]
        Scheduled = 3,
        [EnumMember(Value = "Cliente não compareceu")]
        [Description("Cliente não compareceu")]
        DidNotAttend = 4,
        [EnumMember(Value = "Agendamento concluído")]
        [Description("Agendamento concluído")]
        ScheduleCompleted = 5,
        [EnumMember(Value = "Aguardando orçamento")]
        [Description("Aguardando orçamento")]
        WaitingForBudget = 6,
        [EnumMember(Value = "Orçamento enviado")]
        [Description("Orçamento enviado")]
        BudgetSent = 7,
        [EnumMember(Value = "Aguardando aprovação de orçamento")]
        [Description("Aguardando aprovação de orçamento")]
        WaitingForBudgetApproval = 8,
        [EnumMember(Value = "Orçamento aprovado")]
        [Description("Orçamento aprovado")]
        BudgetApproved = 9,
        [EnumMember(Value = "Orçamento aprovado parcialmente")]
        [Description("Orçamento aprovado parcialmente")]
        BudgetPartiallyApproved = 10,
        [EnumMember(Value = "Orçamento reprovado")]
        [Description("Orçamento reprovado")]
        BudgetDisapprove = 11,
        [EnumMember(Value = "Aguardando pagamento")]
        [Description("Aguardando pagamento")]
        WaitingForPayment = 12,
        [EnumMember(Value = "Pagamento aprovado")]
        [Description("Pagamento aprovado")]
        Paid = 13,
        [EnumMember(Value = "Pagamento reprovado")]
        [Description("Pagamento reprovado")]
        PaymentRejected = 14,
        [EnumMember(Value = "Aguardando início")]
        [Description("Aguardando início")]
        WaitingStart = 15,
        [EnumMember(Value = "Serviço em andamento")]
        [Description("Serviço em andamento")]
        ServiceInProgress = 16,
        [EnumMember(Value = "Aguardando peças")]
        [Description("Aguardando peças")]
        WaitingForPart = 17,
        [EnumMember(Value = "Serviço concluído")]
        [Description("Serviço concluído")]
        ServiceCompleted = 18,
        [EnumMember(Value = "Aguardando aprovação do serviço")]
        [Description("Aguardando aprovação do serviço")]
        WaitingForServiceApproval = 19,
        [EnumMember(Value = "Serviço reprovado pelo cliente")]
        [Description("Serviço reprovado pelo cliente")]
        ServiceReprovedByUser = 20,
        [EnumMember(Value = "Contestação da Oficina")]
        [Description("Contestação da Oficina")]
        WorkshopDispute = 21,
        [EnumMember(Value = "Serviço aprovado pelo cliente")]
        [Description("Serviço aprovado pelo cliente")]
        ServiceApprovedByUser = 22,
        [EnumMember(Value = "Serviço aprovado pelo administrador")]
        [Description("Serviço aprovado pelo administrador")]
        ServiceApprovedByAdmin = 23,
        [EnumMember(Value = "Serviço aprovado parcialmente pelo administrador")]
        [Description("Serviço aprovado parcialmente pelo administrador")]
        ServiceApprovedPartiallyByAdmin = 24,
        [EnumMember(Value = "Serviço reprovado pelo administrador")]
        [Description("Serviço reprovado pelo administrador")]
        ServiceReprovedByAdmin = 25,
        [EnumMember(Value = "Serviço concluído com sucesso")]
        [Description("Serviço concluído com sucesso")]
        ServiceFinished = 26
    }

    public enum SchedulingStatusTitle
    {
        [EnumMember(Value = "Agendamento")]
        [Description("Agendamento")]
        Scheduling = 0,
        [EnumMember(Value = "Orçamento")]
        [Description("Orçamento")]
        Budget = 1,
        [EnumMember(Value = "Pagamento")]
        [Description("Pagamento")]
        Payment = 2,
        [EnumMember(Value = "Serviço")]
        [Description("Serviço")]
        Service = 3,
        [EnumMember(Value = "Aprovação")]
        [Description("Aprovação")]
        Approval = 4,
        [EnumMember(Value = "Concluído")]
        [Description("Concluído")]
        Completed = 5
    }

    public enum ConfirmStatus
    {
        [EnumMember(Value = "Aprovado")]
        [Description("Aprovado")]
        Approved = 0,
        [EnumMember(Value = "Reprovado")]
        [Description("Reprovado")]
        Disapprove = 1
    }

    public enum MenuItem
    {
        [EnumMember(Value = "Dashboard")]
        [Description("Dashboard")]
        Dashboard = 0,
        [EnumMember(Value = "Perfis de acesso")]
        [Description("Perfis de acesso")]
        AccessProfiles = 1,
        [EnumMember(Value = "Administradores")]
        [Description("Administradores")]
        Administrators = 2,
        [EnumMember(Value = "Serviços")]
        [Description("Serviços")]
        Services = 3,
        [EnumMember(Value = "Taxas")]
        [Description("Taxas")]
        Fees = 4,
        [EnumMember(Value = "Oficinas")]
        [Description("Oficinas")]
        Workshops = 5,
        [EnumMember(Value = "Lista de clientes")]
        [Description("Lista de clientes")]
        Users = 6,
        [EnumMember(Value = "Solicitações de serviço")]
        [Description("Solicitações de serviço")]
        ServiceRequests = 7,
        [EnumMember(Value = "Relatório Financeiro")]
        [Description("Relatório Financeiro")]
        FinancialReport = 8,
        [EnumMember(Value = "Notificações")]
        [Description("Notificações")]
        Notifications = 9,
        [EnumMember(Value = "Avaliações")]
        [Description("Avaliações")]
        Reviews = 10
    }

    public enum SubMenuItem
    {

    }
}
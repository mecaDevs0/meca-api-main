using System;

namespace Meca.Domain
{
    public class DefaultMessages
    {

        /*REQUIRED*/
        public const string FieldRequired = "Informe o campo {0}";
        public const string Minlength = "Informe no mínimo {1} caracteres no campo {0}.";
        public const string Maxlength = "Informe no máximo {1} caracteres no campo {0}.";
        public const string Range = "Informe no mínimo {1} e no maxímo {2} caracteres no campo {0}.";
        public const string RangeValue = "Informe um valor entre {1} e {2} no campo {0}.";
        public const string RequiredPassword = "Informe o campo senha.";

        /*CUSTOM MESSAGES*/
        public const string ProfileBlocked = "Usuário bloqueado. entre em contato com suporte";
        public const string UserAdministratorBlocked = "Acesso bloqueado. entre em contato com suporte";
        public const string ProfileUnRegistered = "Informe um e-mail registrado.";
        public const string AwaitApproval = "Seu perfil está sendo analizado, logo entraremos em contato";
        public const string PasswordNoMatch = "Senha atual não confere com a informada";
        public const string ConfirmPasswordNoMatch = "Confirmação de senha não confere com a nova senha informada";
        public const string InvalidRegisterAddress = "Não é possível registrar esse tipo de endereço.";
        public const string BudgetServicesEmpty = "Para aprovar orçamento deve selecionar ao menos um serviço.";

        /*IN USE*/
        public const string CpfInUse = "Cpf em uso.";
        public const string CarPlateInUse = "Placa em uso.";
        public const string CnpjInUse = "Cnpj em uso.";
        public const string PhoneInUse = "Telefone em uso.";
        public const string LoginInUse = "Login em uso.";
        public const string EmailInUse = "E-mail em uso.";
        public const string AppleInUse = "Já existe um usuário com essa conta da apple.";
        public const string FacebookInUse = "Já existe um usuário com essa conta do facebook.";
        public const string GoogleIdInUse = "Já existe um usuário com essa conta do google plus.";
        public const string ServiceInUse = "Já existe um serviço com esse nome.";
        public const string VehicleInUse = "Já existe um veículo com essa placa.";
        public const string SchedulingInUse = "Já existe um agendamento com essa data.";

        /*INVALID*/
        public const string InvalidCredentials = "Credênciais inválidas.";
        public const string InvalidAction = "Você não tem permissão para realizar essa ação.";
        public const string EmailInvalid = "Informe um e-mail válido.";
        public const string CpfInvalid = "Informe um cpf válido.";
        public const string CnpjInvalid = "Informe um cnpj válido.";
        public const string PhoneInvalid = "Informe um telefone válido.";
        public const string InvalidIdentifier = "Formato de id inválido.";
        public const string InvalidEntityMap = "Mapeamento inválido.";
        public const string InvalidLogin = "Login e/ou senha inválidos.";
        public const string CreditCardNumberRequired = "Número de cartão inválido.";
        public const string MonthInvalid = "Mês de vencimento inválido.";
        public const string YearInvalid = "Ano de vencimento inválido.";
        public const string InvalidCvv = "Código de segurança inválido.";

        /*NOT FOUND*/
        public const string UserAdministratorNotFound = "Usuário de acessso não encontrado";
        public const string ProfileNotFound = "Usuário não encontrado";
        public const string CreditCardNotFound = "Cartão não encontrado";
        public const string CreditCardNotFoundIugu = "Forma de pagamento não encontrada";
        public const string ZipCodeNotFound = "Cep não encontrado";
        public const string Updated = "Alterações salvas com sucesso";
        public const string Registered = "Cadastrado com sucesso";
        public const string Deleted = "Removido com sucesso";
        public const string EmptyProviderId = "Informe o providerId";
        public const string AppleIdInUse = "AppleId em uso, tente fazer login.";
        public const string AccessBlocked = "Acesso bloqueado. entre em contato com suporte";
        public const string AccessBlockedWithReason = "Acesso bloqueado. {0}";
        public const string OnlyAdministrator = "Você precisa ser administrador para realizar essa ação";
        public const string MessageException = "Ocorreu um erro, verifique e tente novamente";
        public const string EntidadeDisabled = "Essa entidade não está mais disponível na plataforma";
        public const string DateFormatInvalid = "Informe uma data no formato DD/MM/AAAA.";
        public const string DefaultError = "Ocorreu um erro, verifique e tente novamente";
        public const string PasswordChanged = "Senha alterada com sucesso.";
        public const string VerifyYourEmail = "Verifique seu e-mail";
        public const string Available = "Disponível";
        public const string FileNotFound = "Nenhum arquivo encontrado";
        public const string LargeFile = "Arquivo muito grande, o arquivo deve ter no máximo {{ limitMb }}";
        public const string InvalidFile = "Arquivo não permitido!";
        public const string OneOrMoreFilesToLarge = "Um ou mais arquivos muito grande";
        public const string RequiredZipCode = "Informe o cep";
        public const string ZipCodeNotFoundOrOffline = "Cep não encontrado ou serviço offline";
        public const string RequiredCpf = "Informe o cpf";
        public const string InvalidTypeProfile = "Tipo de perfil inválido";
        public const string NotificationNotFound = "Notificação não encontrada";
        public const string Sended = "Mensagem enviada com sucesso";
        public const string ErrorOnSendPush = "Ocorreu um erro ao tentar enviar a notificação push";
        public const string InvoiceOfAnotherProject = "Fatura de outro projeto";
        public const string AlreadyVerified = "Essa conta já foi verificada";
        public const string NeedElements = "Selecione pelo menos {1} iten(s) no campo {0}";
        public const string AccessLevelNotFound = "Perfil de acesso não encontrado";
        public const string InvalidDeleteDefault = "Não é pósivel remover, esse item é necessário para o funcionamento do sistema";
        public const string InvalidUpdateDefault = "Não é pósivel atualizar, esse item é necessário para o funcionamento do sistema";
        public const string AlreadyInUse = "Não é possível remover um item em uso";
        public const string SheetNotFound = "Esse arquivo não possui nenhuma guia ou é inválido, verifique o arquivo e tente novamente.";
        public const string FinancialHistoryNotFound = "Histórico financeiro não encontrado";
        public const string ServicesNotFound = "Serviço não encontrado";
        public const string FeesNotFound = "Taxa não encontrada";
        public const string WorkshopNotFound = "Oficina não encontrada";
        public const string WorkshopAgendaNotFound = "Agenda não encontrada";
        public const string VehicleNotFound = "Veículo não encontrado";
        public const string SchedulingNotFound = "Agendamento não encontrado";
        public const string RatingInvalid = "A avaliação deve estar entre 0 e 5.";
        public const string RatingNotFound = "Avaliação não encontrada";
        public const string OnlyWorkshop = "Você precisa ser oficina para realizar essa ação.";
        public const string NotPermission = "Você não possui permissão para realizar essa ação.";
        public const string AppointmentRefused = "Agendamento recusado.";
        public const string AppointmentConfirmed = "O agendamento foi confirmado com sucesso.";
        public const string InvalidPaymentMethod = "Forma de pagamento inválida ou não habilitada.";
        public const string TransactionError = "Não foi possível realizar essa transação, verifique os dados do cartão e tente novamente.";
        public const string TransactionSuccess = "Pagamento realizado com sucesso.";
        public const string SchedulingHistoryNotFound = "Histórico não encontrado";
        public const string FaqNotFound = "Faq não encontrado";
        public const string FaqInUse = "Já existe um faq com essa pergunta";
        public const string WorkshopWithScheduling = "Você não pode realizar essa ação, existem serviços vinculados a essa oficina.";
        public const string FeeNotFound = "Taxas da plataforma não configuradas, entre em contato com suporte.";
        public const string WorkshopNotRegisteredInGateway = "Oficina não cadastrada no gateway, entre em contato com suporte caso persista o erro.";
        public const string DateOfBirthInvalid = "Data de nascimento inválida";
        public const string NotIdentifyedPayment = "Pagamento não identificado. Tente novamente, se o erro persistir entre em contato com suporte.";
        public const string SchedulingServiceNotApproved = "O serviço não foi aprovado. Entre em contato com o suporte.";
        public const string InvoiceNotFound = "Identificador da fatura não encontrado";
        public const string InvoiceFundsNotAvailable = "Saldo da transação ainda não está disponível para split.";
    }
}
using System.Threading.Tasks;

namespace Meca.ApplicationService.Services
{
    public interface IPagBankService
    {
        /// <summary>
        /// Criar conta no PagBank (equivalente ao CreateAsync do Stripe)
        /// </summary>
        Task<PagBankResponse<PagBankAccount>> CreateAccountAsync(PagBankAccountRequest request);

        /// <summary>
        /// Buscar conta por ID (equivalente ao GetByIdAsync do Stripe)
        /// </summary>
        Task<PagBankResponse<PagBankAccount>> GetAccountByIdAsync(string accountId);

        /// <summary>
        /// Atualizar conta (equivalente ao UpdateAsync do Stripe)
        /// </summary>
        Task<PagBankResponse<PagBankAccount>> UpdateAccountAsync(string accountId, PagBankAccountRequest request);

        /// <summary>
        /// Criar conta bancária (equivalente ao CreateBankAccountOptions do Stripe)
        /// </summary>
        Task<PagBankResponse<PagBankBankAccount>> CreateBankAccountAsync(string accountId, PagBankBankAccountRequest request);

        /// <summary>
        /// Buscar conta bancária
        /// </summary>
        Task<PagBankResponse<PagBankBankAccount>> GetBankAccountAsync(string accountId);

        /// <summary>
        /// Deletar conta (equivalente ao DeleteAsync do Stripe)
        /// </summary>
        Task<PagBankResponse<bool>> DeleteAccountAsync(string accountId);
    }
}




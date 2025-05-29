using System.Collections.Generic;
using Meca.Data.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace Meca.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Profile : TEntity<Profile>
    {
        public Profile()
        {
            CreditCards = [];
            DeviceId = [];
        }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Cpf { get; set; }
        public string Password { get; set; }
        public string LastPassword { get; set; }
        public string ProviderId { get; set; }
        public TypeProvider TypeProvider { get; set; }
        public string Reason { get; set; }
        public string Photo { get; set; }
        public string Phone { get; set; }

        /*USO DA IUGU*/
        public string AccountKey { get; set; }
        public string AccountKeyDev { get; set; }
        public List<string> CreditCards { get; set; }
        public List<string> DeviceId { get; set; }
        public bool? IsAnonymous { get; set; }

        public string GetAccountKey(bool isSandBox)
            => isSandBox ? AccountKeyDev : AccountKey;

        /*ADDRESS INFO*/
        public string ZipCode { get; set; }
        public string StreetAddress { get; set; }
        public string Number { get; set; }
        public string CityName { get; set; }
        public string CityId { get; set; }
        public string StateName { get; set; }
        public string StateUf { get; set; }
        public string StateId { get; set; }
        public string Neighborhood { get; set; }
        public string Complement { get; set; }
        public string ExternalId { get; set; }
        public long? LastSyncStripe { get; set; }

        /*DATABANK*/
        // public string AccountId { get; set; }
        // public string LiveKey { get; set; }
        // public string TestKey { get; set; }
        // public string UserApiKey { get; set; }
        // public long? LastRequestVerification { get; set; }
        // public bool HasDataBank { get; set; }
        // public long? LastConfirmDataBank { get; set; }
        // public string AccountableName { get; set; }
        // public string AccountableCpf { get; set; }
        // public string BankCnpj { get; set; }
        // public string Bank { get; set; }
        // public string BankName { get; set; }
        // public string BankAccount { get; set; }
        // public string BankAgency { get; set; }
        // public TypeAccount TypeAccount { get; set; }
        // public TypePersonBank PersonType { get; set; }
        // public DataBankStatus DataBankStatus { get; set; } = DataBankStatus.Uninformed;

    }
}
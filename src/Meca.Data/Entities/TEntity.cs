using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using UtilityFramework.Infra.Core3.MongoDb.Data.Modelos;

namespace Meca.Data.Entities
{
    public class TEntity<T> : ModelBase where T : class
    {
        [BsonIgnore]
        [JsonIgnore]
        public ValidationResult _validationResults { get; private set; }

        public void SetId(string id)
        {
            _id = ObjectId.Parse(id);
        }
        public string GetStringId() => _id.ToString();



        public string ReturnValidationsToString()
        {
            var erros = _validationResults?.Errors.Select(x => x.ErrorMessage).ToList();

            return erros != null ? string.Join(',', erros) : "";
        }

        public bool isValid(T entity, AbstractValidator<T> validator)
        {
            _validationResults = validator.Validate(entity);

            return _validationResults.IsValid;
        }



        [JsonIgnore]
        public override string CollectionName => (typeof(T).Name);
    }
}

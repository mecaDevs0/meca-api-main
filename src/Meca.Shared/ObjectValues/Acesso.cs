using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace Meca.Shared.ObjectValues
{
    public class Acesso
    {
        public Acesso(string usuarioId, int typeToken)
        {
            UserId = usuarioId;
            TypeToken = typeToken;
        }

        public bool IsAdmin { get; private set; } = false;
        public int TypeToken { get; private set; }
        public string UserId { get; private set; }

        public ObjectId UserObjectId
        {
            get
            {
                return string.IsNullOrWhiteSpace(UserId) == false ? ObjectId.Parse(UserId) : ObjectId.Empty;
            }
            private set { }
        }

        /// <summary>
        /// MÉTODO SOMENTE UTILIZADO PELO TEST UNITÁRIO PARA SETA O ID DO ADMINISTRADOR
        /// </summary>
        /// <param name="id"></param>
        public void SetUsuarioId(string id, int? typeToken)
        {
            UserId = id;
            if (typeToken != null)
                TypeToken = typeToken.GetValueOrDefault();
        }

        /// <summary>
        /// DEFINIR ACESSO COMO ADMINISTRADOR
        /// </summary>
        public void SetAdministrador()
        {
            IsAdmin = true;
        }
    }
}

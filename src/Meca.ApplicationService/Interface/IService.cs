using System.Collections.Generic;
using FluentValidation.Results;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.ApplicationService.Interface
{
    public interface IService
    {
        string ReturnValidationsToString();
        ValidationResult ReturnValidations();
        ReturnViewModel GetReturnViewModel();
        Acesso SetAccess(IHttpContextAccessor contextAccessor);
        void SetModelState(ModelStateDictionary modelState);
        IList<ValidationFailure> GetErrors();
        void MergeErrors(IService service);
    }
}

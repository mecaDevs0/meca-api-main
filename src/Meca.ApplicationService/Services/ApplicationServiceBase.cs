using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Meca.ApplicationService.Interface;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OfficeOpenXml;
using Stripe;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Services.Stripe.Core3;
using UtilityFramework.Services.Stripe.Core3.Models;

namespace Meca.ApplicationService.Services
{
    public abstract class ApplicationServiceBase<T> where T : class
    {
        protected FluentValidation.Results.ValidationResult _validationResults { get; set; }
        public ReturnViewModel _returnViewModel { get; private set; }
        public HttpRequest _request { get; private set; }
        public Acesso _access { get; private set; }
        public ModelStateDictionary _modelState { get; private set; }
        public IHttpContextAccessor _contextAcessor;
        public string[] _validFields { get; private set; }
        public string[] _jsonBodyFields { get; private set; }
        public readonly List<string> _acceptedFiles = [".xls", ".xlsx"];
        public bool _isUnitTest { get; private set; } = false;

        public bool ModelIsValid(bool onlyValidFields = false, string[] customValidFields = null, string[] ignoredFields = null, string customStart = null)
        {

            _jsonBodyFields = _contextAcessor?.GetFieldsFromBody() ?? new string[0];
            _validFields = onlyValidFields ? customValidFields ?? _jsonBodyFields : null;

            if (ignoredFields != null && ignoredFields.Length > 0)
            {
                _validFields = _validFields.Where(x => ignoredFields.Count(y => y.EqualsIgnoreCase(x)) == 0).ToArray();
            }

            var invalidState = onlyValidFields ? _modelState.ValidModelStateOnlyFields(_validFields) : _modelState.ValidModelState(ignoredFields, null);

            if (invalidState != null)
            {
                if (string.IsNullOrEmpty(customStart) == false)
                    invalidState.Message = $"{customStart} {invalidState.Message}".Trim();

                _returnViewModel = invalidState;
                return false;
            }

            return true;
        }

        public bool ModelIsValid(List<System.ComponentModel.DataAnnotations.ValidationResult> validationResults, bool onlyValidFields = false, string[] customValidFields = null, string[] ignoredFields = null, string startMessage = null)
        {

            _jsonBodyFields = _contextAcessor?.GetFieldsFromBody() ?? new string[0];
            _validFields = onlyValidFields ? customValidFields ?? _jsonBodyFields : null;

            if (ignoredFields != null && ignoredFields.Length > 0)
            {
                _validFields = _validFields.Where(x => ignoredFields.Count(y => y.EqualsIgnoreCase(x)) == 0).ToArray();
            }

            if (validationResults != null && validationResults.Count > 0)
            {
                var errors = new Dictionary<string, string>();

                for (int i = 0; i < validationResults.Count; i++)
                {
                    var validationItem = validationResults[i];
                    var memberName = validationItem.MemberNames.FirstOrDefault() ?? "custom_error";

                    if ((Equals(memberName, "custom_error") && errors.ContainsKey(memberName)) || (ignoredFields != null && ignoredFields.Count(x => x.ContainsIgnoreCase(memberName)) > 0))
                        continue;

                    if (onlyValidFields == false || (_validFields != null && _validFields.Count(x => x.ContainsIgnoreCase(memberName)) > 0))
                        errors.Add(memberName, validationItem.ErrorMessage);
                }

                if (errors.Count == 0)
                    return true;

                _returnViewModel = new ReturnViewModel()
                {
                    Erro = true,
                    Errors = errors,
                    Message = $"{startMessage?.TrimStart() ?? ""} {errors.FirstOrDefault().Value}".Trim()
                };

                return false;
            }

            return true;
        }

        public bool ModelIsValid<TEntity>(TEntity entity, bool onlyValidFields = false, string[] customValidFields = null, string[] ignoredFields = null, string customStart = null, bool customValidation = false) where TEntity : new()
        {
            var context = new ValidationContext(entity);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            _ = Validator.TryValidateObject(entity, context, validationResults, true);

            return _isUnitTest == false && customValidation == false && _modelState != null
                   ? ModelIsValid(onlyValidFields, customValidFields, ignoredFields, customStart)
                   : ModelIsValid(validationResults, onlyValidFields, customValidFields, ignoredFields, customStart);
        }

        public bool ModelIsValid(T entity, AbstractValidator<T> validator)
        {
            _validationResults = validator.Validate(entity);

            return _validationResults.IsValid;
        }
        public ReturnViewModel GetReturnViewModel()
        {
            return _returnViewModel;
        }

        public string ReturnValidationsToString()
        {
            List<string> errors = null;

            /*SE USAR FLUENT VALIDATIONS*/
            if (_validationResults != null && _validationResults.Errors?.Count > 0)
                errors = _validationResults?.Errors?.Select(x => x.ErrorMessage).ToList();

            /*SE USAR MODEL VALIDATIONS*/
            else if (_returnViewModel != null)
                errors = _returnViewModel.Errors.ToDictionary<string>().Values.ToList();

            return errors != null ? errors.CustomJoin() : "";
        }
        public void CreateNotification(string message)
        {
            var notificacao = new ValidationFailure("erro", message);

            _validationResults ??= new FluentValidation.Results.ValidationResult();

            _validationResults.Errors.Add(notificacao);

        }

        public IList<ValidationFailure> GetErrors()
            => _validationResults.Errors;
        public void MergeErrors(IService service)
        {
            var errors = service.GetErrors();
            _validationResults ??= new FluentValidation.Results.ValidationResult();

            for (int i = 0; i < errors.Count; i++)
            {
                _validationResults.Errors.Add(errors[i]);
            }
        }

        public FluentValidation.Results.ValidationResult ReturnValidations()
            => _validationResults;

        public void ClearNotifications()
        {
            _validationResults.Errors.Clear();
        }

        public void SetModelState(ModelStateDictionary modelState)
        {
            _modelState = modelState;
        }

        public Acesso SetAccess(IHttpContextAccessor contextAcessor)
        {
            _contextAcessor = contextAcessor;

            if (_contextAcessor?.HttpContext == null)
            {
                Console.WriteLine("[APPLICATION_BASE_DEBUG] WARNING: HttpContext é null");
                _access = new Acesso(null, 0);
                return _access;
            }

            var context = _contextAcessor.HttpContext;

            string usuarioId = context.Request.GetClaimFromToken("sub");
            string typeTokenStr = context.Request.GetClaimFromToken(ClaimTypes.Role);

            _ = int.TryParse(typeTokenStr, out int typeTokenValue);

            bool isAdmin = typeTokenValue == (int)TypeProfile.UserAdministrator;

            _access = new Acesso(usuarioId, typeTokenValue);

            if (isAdmin)
                _access.SetAdministrador();

            return _access;
        }

        /*MÉTODO SOMENTE UTILIZADO POR TESTES DE UNIDADE*/
        public void SetAccessTest(Acesso acesso)
        {
            _access = acesso;

            if (acesso.TypeToken == (int)TypeProfile.UserAdministrator)
                _access.SetAdministrador();

            _isUnitTest = true;

        }

        public void OnlyAdministrator()
        {
            if (_access.IsAdmin == false)
                throw new InvalidOperationException(DefaultMessages.OnlyAdministrator);
        }

        public async Task<List<TDestination>> ReadAndValidationExcel<TDestination>(IFormFile file, bool checkIsValid = false, int worksheetItem = 0) where TDestination : new()
        {
            var response = new List<TDestination>();
            try
            {
                using ExcelPackage package = new ExcelPackage(file.OpenReadStream());
                if (package.Workbook.Worksheets == null || package.Workbook.Worksheets.Count == 0)
                    throw new Exception(DefaultMessages.SheetNotFound);

                ExcelWorksheet sheet = package.Workbook.Worksheets[worksheetItem];

                response = await SheetToClass<TDestination>(sheet, checkIsValid);
            }
            catch (Exception)
            {
                throw;
            }
            return response;
        }

        public async Task<List<TDestination>> ReadAndValidationExcel<TDestination>(string path, bool checkIsValid = false, int worksheetItem = 0) where TDestination : new()
        {
            var response = new List<TDestination>();

            try
            {
                var file = new FileInfo(path);
                using ExcelPackage package = new ExcelPackage(file);
                if (package.Workbook.Worksheets == null || package.Workbook.Worksheets.Count == 0)
                    throw new Exception(DefaultMessages.SheetNotFound);

                ExcelWorksheet sheet = package.Workbook.Worksheets[worksheetItem];

                response = await SheetToClass<TDestination>(sheet, checkIsValid);
            }
            catch (Exception)
            {

                throw;
            }
            return response;
        }

        private async Task<List<TDestination>> SheetToClass<TDestination>(ExcelWorksheet sheet, bool checkIsValid) where TDestination : new()
        {

            var response = new List<TDestination>();

            try
            {
                var listEntityViewModel = sheet.ConvertSheetToObjects<TDestination>();

                var properties = typeof(TDestination).GetProperties().ToList();

                var searchZipCode = properties.Count(x => x.Name == "ZipCode") > 0;

                for (int i = 0; i < listEntityViewModel.Count; i++)
                {
                    var ignoredFields = new List<string>();

                    if (checkIsValid && ModelIsValid(listEntityViewModel[i], ignoredFields: ignoredFields.ToArray(), customStart: $"Erro na linha {i + 1}") == false)
                        return response;

                    if (searchZipCode && ignoredFields.Count(x => x == "ZipCode") == 0)
                    {
                        var zipCode = Utilities.GetValueByProperty<string>(listEntityViewModel[i], "ZipCode")?.OnlyNumbers().PadLeft(8, '0');

                        if (string.IsNullOrEmpty(zipCode) == false)
                        {
                            var responseInfo = await Utilities.GetInfoZipCode(zipCode);

                            if (responseInfo != null)
                            {
                                var propertiesAddress = responseInfo.GetType().GetProperties().ToList();

                                for (int a = 0; a < propertiesAddress.Count; a++)
                                {
                                    if (properties.Count(x => x.Name == propertiesAddress[a].Name) > 0 && Utilities.GetValueByProperty(listEntityViewModel[i], propertiesAddress[a].Name) == null)
                                        Utilities.SetPropertyValue(listEntityViewModel[i], propertiesAddress[a].Name, Utilities.GetValueByProperty(responseInfo, propertiesAddress[a].Name));
                                }

                                Utilities.SetPropertyValue(listEntityViewModel[i], "ZipCode", zipCode.OnlyNumbers().PadLeft(8, '0'));
                            }
                        }
                    }
                }

                response = listEntityViewModel.ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }

        public static async Task<StripeBaseResponse<TData>> HandleActionAsync<TData>(
            Func<Task<TData>> paymentAction)
        {
            var response = new StripeBaseResponse<TData>();

            try
            {
                var result = await paymentAction();

                response.Success = true;
                response.Data = result;
            }
            catch (StripeException ex)
            {
                response.ErrorMessage = $"[Stripe Error] {ex.StripeError.Message}";
                response.ErrorCode = ex.StripeError.Code;
                response.ErrorType = ex.StripeError.Type;
                response.ErrorParam = ex.StripeError.Param;
                response.StackTrace = ex.StackTrace;
            }
            catch (CustomError ex)
            {
                response.ErrorMessage = ex.Message;
                response.ErrorType = "custom_error";
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
                response.ErrorType = "exception";
            }

            return response;
        }

        public static async Task<StripeBaseResponse> HandleActionAsync(
            Func<Task> paymentAction)
        {
            var response = new StripeBaseResponse();

            try
            {
                await paymentAction();
                response.Success = true;
            }
            catch (StripeException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"[Stripe Error] {ex.StripeError.Message}";
                response.ErrorCode = ex.StripeError.Code;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
            }

            return response;
        }
    }
}

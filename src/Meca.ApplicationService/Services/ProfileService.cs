using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Stripe.Core3;
using UtilityFramework.Services.Stripe.Core3.Interfaces;
using UtilityFramework.Services.Stripe.Core3.Models;

namespace Meca.ApplicationService.Services
{
    public class ProfileService : ApplicationServiceBase<Data.Entities.Profile>, IProfileService
    {
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<Notification> _notificationRepository;
        private readonly ISenderMailService _senderMailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly IStripeCustomerService _stripeCustomerService;
        private IConfiguration _configuration;
        private readonly IMapper _mapper;

        /*Construtor utilizado por testes de unidade*/
        public ProfileService(IHostingEnvironment env, IMapper mapper, IConfiguration configuration, Acesso acesso, IBusinessBaseAsync<Data.Entities.Profile> profileRepository, string testUnit)
        {
            _profileRepository = profileRepository;
            _mapper = mapper;
            _configuration = configuration;
            SetAccessTest(acesso);
        }

        public ProfileService(
            IMapper mapper,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<Notification> notificationRepository,
            ISenderMailService senderMailService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _profileRepository = profileRepository;
            _notificationRepository = notificationRepository;
            _senderMailService = senderMailService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            // _stripeCustomerService = stripeCustomerService;
        }

        public async Task<List<ProfileViewModel>> GetAll()
        {
            var listEntityData = await _profileRepository.FindAllAsync(Util.Sort<Data.Entities.Profile>().Ascending(nameof(Data.Entities.Profile.FullName)));

            return _mapper.Map<List<ProfileViewModel>>(listEntityData);
        }
        public async Task<List<T>> GetAll<T>(ProfileFilterViewModel filterView) where T : class
        {

            filterView.Page = Math.Max(1, filterView.Page.GetValueOrDefault());

            if (filterView.Limit == null || filterView.Limit.GetValueOrDefault() == 0)
                filterView.Limit = 30;

            var builder = Builders<Data.Entities.Profile>.Filter;

            var conditions = new List<FilterDefinition<Data.Entities.Profile>>();

            /*FILTRO DE DELETE LOGICAL*/
            ///conditions.Add(builder.Eq(x => x.Disabled, null));

            if (string.IsNullOrEmpty(filterView.FullName) == false)
                conditions.Add(builder.Regex(x => x.FullName, new BsonRegularExpression(filterView.FullName, "i")));

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listEntityData = await _profileRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions<Data.Entities.Profile>(filterView, Util.Sort<Data.Entities.Profile>().Ascending(x => x.Created)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listEntityData);
        }

        public async Task<ProfileViewModel> Detail(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var userId = _access.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var profileEntity = await _profileRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                return _mapper.Map<ProfileViewModel>(profileEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ProfileViewModel> GetInfo()
        {
            try
            {
                var userId = _access.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var profileEntity = await _profileRepository.FindByIdAsync(userId).ConfigureAwait(false);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                // profileEntity = await SyncProfileWithStripe(profileEntity);

                var responseVm = _mapper.Map<ProfileViewModel>(profileEntity);

                responseVm.TotalNotificationNoRead = await _notificationRepository.CountLongAsync(x =>
                    x.ReferenceId == userId &&
                    x.DateRead == null &&
                    x.TypeReference == TypeProfile.Profile
                );

                return responseVm;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return false;
                }

                var profileEntity = await _profileRepository.FindByIdAsync(id);

                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return false;
                }

                await _profileRepository.DeleteOneAsync(id);

                // if (string.IsNullOrEmpty(profileEntity.ExternalId) == false)
                // {
                //     await _stripeCustomerService.DeleteCustomerAsync(profileEntity.ExternalId);
                // }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteByEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || email.ValidEmail() == false)
                {
                    CreateNotification(DefaultMessages.EmailInvalid);
                    return false;
                }

                var profileEntity = await _profileRepository.FindOneByAsync(x => x.Email == email.ToLower());

                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return false;
                }

                await _profileRepository.DeleteOneAsync(profileEntity.GetStringId());

                // if (string.IsNullOrEmpty(profileEntity.ExternalId) == false)
                // {
                //     await _stripeCustomerService.DeleteCustomerAsync(profileEntity.ExternalId);
                // }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> Register(ProfileRegisterViewModel model)
        {
            try
            {
                var ignoredFields = new List<string>();

                if (model.TypeProvider != TypeProvider.Password)
                {
                    ignoredFields.Add(nameof(model.Email));
                    ignoredFields.Add(nameof(model.Password));
                }

                if (ModelIsValid(model, ignoredFields: [.. ignoredFields]) == false)
                    return null;

                var claimRole = Util.SetRole(TypeProfile.Profile);

                if (model.TypeProvider != TypeProvider.Password)
                {
                    if (string.IsNullOrEmpty(model.ProviderId))
                    {
                        CreateNotification(DefaultMessages.EmptyProviderId);
                        return null;
                    }

                    var messageError = "";

                    if (await _profileRepository.CheckByAsync(x => x.ProviderId == model.ProviderId))
                    {
                        messageError = model.TypeProvider switch
                        {
                            TypeProvider.Apple => DefaultMessages.AppleIdInUse,
                            _ => DefaultMessages.FacebookInUse,
                        };

                        CreateNotification(messageError);
                        return null;
                    }

                    model.Login = model.Email;
                }

                if (string.IsNullOrEmpty(model.Email) == false && await _profileRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.EmailInUse);
                    return null;
                }

                if (string.IsNullOrEmpty(model.Cpf) == false && await _profileRepository.CheckByAsync(x => x.Cpf == model.Cpf).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.CpfInUse);
                    return null;
                }

                var profileEntity = _mapper.Map<Data.Entities.Profile>(model);

                if (string.IsNullOrEmpty(model.Password) == false)
                    profileEntity.Password = Utilities.GerarHashMd5(model.Password);

                var entityId = await _profileRepository.CreateAsync(profileEntity).ConfigureAwait(false);

                var claims = new Claim[]
                {
                    claimRole,
                };

                return TokenProviderMiddleware.GenerateToken(entityId, false, claims);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> Token(LoginViewModel model)
        {
            try
            {
                // Debug: Verificar se model não é null
                if (model == null)
                {
                    CreateNotification("Model is null");
                    return null;
                }

                if (string.IsNullOrEmpty(model.RefreshToken) == false)
                    return TokenProviderMiddleware.RefreshToken(model.RefreshToken);

                var ignoreFields = new List<string>();

                if (model.TypeProvider != TypeProvider.Password)
                {
                    if (string.IsNullOrEmpty(model.ProviderId))
                    {
                        CreateNotification(DefaultMessages.EmptyProviderId);
                        return null;
                    }

                    ignoreFields.Add(nameof(model.Email));
                    ignoreFields.Add(nameof(model.Password));
                }

                // Debug: Verificar se _profileRepository não é null
                if (_profileRepository == null)
                {
                    CreateNotification("_profileRepository is null");
                    return null;
                }

                if (ModelIsValid(model, ignoredFields: [.. ignoreFields]) == false)
                    return null;

                var claimRole = Util.SetRole(TypeProfile.Profile);

                Data.Entities.Profile profileEntity;
                if (model.TypeProvider != TypeProvider.Password)
                {
                    profileEntity = await _profileRepository.FindOneByAsync(x => x.ProviderId == model.ProviderId)
                        .ConfigureAwait(false);
                }
                else
                {
                    // Debug: Verificar se Email e Password não são null
                    if (string.IsNullOrEmpty(model.Email))
                    {
                        CreateNotification("Email is null or empty");
                        return null;
                    }

                    if (string.IsNullOrEmpty(model.Password))
                    {
                        CreateNotification("Password is null or empty");
                        return null;
                    }

                    profileEntity = await _profileRepository
                      .FindOneByAsync(x => x.Email == model.Email && x.Password == Utilities.GerarHashMd5(model.Password)).ConfigureAwait(false);
                }

                if (profileEntity == null || profileEntity.Disabled != null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                if (profileEntity.DataBlocked != null)
                {
                    CreateNotification(string.Format(DefaultMessages.AccessBlockedWithReason,
                    (string.IsNullOrEmpty(profileEntity.Reason) ? $"Motivo {profileEntity.Reason}" : "").Trim()));
                    return null;
                }

                var claims = new Claim[]
                {
                    claimRole,
                };

                return TokenProviderMiddleware.GenerateToken(profileEntity._id.ToString(), false, claims);
            }
            catch (Exception ex)
            {
                // Debug: Log da exceção
                CreateNotification($"Exception in Token: {ex.Message}");
                throw;
            }
        }

        public async Task<string> BlockUnBlock(BlockViewModel model)
        {
            try
            {
                if (ModelIsValid(model, true) == false)
                    return null;

                var profileEntity = await _profileRepository.FindByIdAsync(model.TargetId);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                profileEntity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;
                profileEntity.Reason = model.Block ? model.Reason : null;

                await _profileRepository.UpdateAsync(profileEntity);

                return model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var userId = _access.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return false;
                }

                var profileEntity = await _profileRepository.FindByIdAsync(userId).ConfigureAwait(false);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return false;
                }

                if (profileEntity.Password != Utilities.GerarHashMd5(model.CurrentPassword))
                {
                    CreateNotification(DefaultMessages.PasswordNoMatch);
                    return false;
                }

                profileEntity.LastPassword = profileEntity.Password;
                profileEntity.Password = Utilities.GerarHashMd5(model.NewPassword);

                _profileRepository.Update(profileEntity);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DtResult<ProfileViewModel>> LoadData(DtParameters model)
        {
            try
            {
                var response = new DtResult<ProfileViewModel>();

                var builder = Builders<Data.Entities.Profile>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Profile>> { builder.Where(x => x.Disabled == null) };

                var columns = model.Columns != null 
                    ? model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray()
                    : new string[] { };

                var totalRecords = (int)await _profileRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = (model.Order != null && model.Columns != null) 
                    ? Util.MapSort<Data.Entities.Profile>(model.Order, model.Columns, model.SortOrder)
                    : Util.Sort<Data.Entities.Profile>().Ascending(x => x.Created);

                var searchValue = model.Search?.Value ?? "";
                var result = await _profileRepository
                   .LoadDataTableAsync(searchValue, sortBy, model.Start, model.Length, conditions, columns);

                var totalRecordsFiltered = !string.IsNullOrEmpty(searchValue)
                   ? (int)await _profileRepository.CountSearchDataTableAsync(searchValue, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<ProfileViewModel>>(result);
                response.Draw = model.Draw;
                response.RecordsFiltered = totalRecordsFiltered;
                response.RecordsTotal = totalRecords;

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RegisterUnRegisterDeviceId(PushViewModel model)
        {
            try
            {
                Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Iniciando registro/remoção de dispositivo");
                Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] DeviceId: {model.DeviceId}");
                Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] IsRegister: {model.IsRegister}");
                
                if (ModelIsValid(model, true) == false)
                {
                    Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Modelo inválido");
                    return false;
                }

                var userId = _access.UserId;
                Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] UserId: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] UserId está vazio");
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return false;
                }

                _ = Task.Run(() =>
               {
                   if (string.IsNullOrEmpty(model.DeviceId) == false)
                   {
                       Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Executando operação no banco de dados");
                       if (model.IsRegister)
                       {
                           Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Registrando dispositivo {model.DeviceId} para usuário {userId}");
                           _profileRepository.UpdateMultiple(Query<Data.Entities.Profile>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Data.Entities.Profile>().AddToSet(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                       }
                       else
                       {
                           Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Removendo dispositivo {model.DeviceId} do usuário {userId}");
                           _profileRepository.UpdateMultiple(Query<Data.Entities.Profile>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Data.Entities.Profile>().Pull(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                       }
                       Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Operação concluída com sucesso");
                   }
                   else
                   {
                       Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] DeviceId está vazio, operação ignorada");
                   }
               });

                Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Método concluído com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Erro: {ex.Message}");
                Console.WriteLine($"[DEVICE_REGISTRATION_DEBUG] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ProfileViewModel> UpdatePatch(string id, ProfileRegisterViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                var userId = id ?? model?.Id ?? _access.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var _id = ObjectId.Parse(userId);

                if (string.IsNullOrEmpty(model.Email) == false && await _profileRepository.CheckByAsync(x => x.Email == model.Email && x._id != _id))
                {
                    CreateNotification(DefaultMessages.EmailInUse);
                    return null;
                }

                if (string.IsNullOrEmpty(model.Cpf) == false && await _profileRepository.CheckByAsync(x => x.Cpf == model.Cpf && x._id != _id))
                {
                    CreateNotification(DefaultMessages.CpfInUse);
                    return null;
                }

                var profileEntity = await _profileRepository.FindByIdAsync(userId).ConfigureAwait(false);

                profileEntity.SetIfDifferent(model, validOnly, _mapper);

                profileEntity = await _profileRepository.UpdateAsync(profileEntity).ConfigureAwait(false);

                return _mapper.Map<ProfileViewModel>(profileEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ForgotPassword(LoginViewModel model)
        {
            try
            {
                var validOnly = new string[1] { nameof(model.Email) };

                if (ModelIsValid(model, true, validOnly) == false)
                    return false;

                var profileEntity = await _profileRepository.FindOneByAsync(x => x.Disabled == null && x.Email == model.Email).ConfigureAwait(false);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileUnRegistered);
                    return false;
                }

                var newPassword = Utilities.RandomInt(8);
                var title = "Nova senha de acesso";
                var subject = "Lembrete de senha";
                var dataBody = Util.GetTemplateVariables();

                dataBody.Add("{{ name }}", profileEntity.FullName.GetFirstName());
                dataBody.Add("{{ email }}", profileEntity.Email);
                dataBody.Add("{{ password }}", newPassword);

                dataBody.Add("{{ title }}", title);
                dataBody.Add("{{ message }}", Util.GetForgotPasswordTemplate(true).ReplaceTag(dataBody));

                var body = _senderMailService.GerateBody("custom", dataBody);

                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(
                        BaseConfig.ApplicationName,
                        profileEntity.Email,
                        body,
                        subject);

                    profileEntity.Password = Utilities.GerarHashMd5(newPassword);
                    await _profileRepository.UpdateAsync(profileEntity);
                });

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckEmail(ValidationViewModel model)
        {
            try
            {
                var validOnly = new string[1] { nameof(model.Email) };

                if (ModelIsValid(model, true, validOnly) == false)
                    return false;

                if (await _profileRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.EmailInUse);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckLogin(ValidationViewModel model)
        {
            try
            {
                var validOnly = new string[1] { nameof(model.Login) };

                if (ModelIsValid(model, true, validOnly) == false)
                    return false;

                if (await _profileRepository.CheckByAsync(x => x.Login == model.Login).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.LoginInUse);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckCpf(ValidationViewModel model)
        {
            try
            {
                var ignoreFields = new List<string>
                {
                    nameof(model.Email)
                };

                if (ModelIsValid(model, ignoredFields: [.. ignoreFields]) == false)
                    return false;

                if (await _profileRepository.CheckByAsync(x => x.Cpf == model.Cpf).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.CpfInUse);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> CheckAll(ValidationViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Cnpj) == false && await _profileRepository.CheckByAsync(x => x.Cpf == model.Cpf).ConfigureAwait(false))
                {
                    return DefaultMessages.CpfInUse;
                }

                if (string.IsNullOrEmpty(model.Login) == false && await _profileRepository.CheckByAsync(x => x.Login == model.Login).ConfigureAwait(false))
                {
                    return DefaultMessages.LoginInUse;
                }

                if (string.IsNullOrEmpty(model.Email) == false && await _profileRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
                {
                    return DefaultMessages.EmailInUse;
                }

                return DefaultMessages.Available;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendEmailRequestingAppHelp(SiteMecaFormViewModel model)
        {
            try
            {
                if (ModelIsValid(model, true) == false)
                    return false;

                var contactEmail = "contato@mecabr.com";
                var title = "Solicitação de Ajuda ao app Meca";
                var subject = "Solicitação de Ajuda ao app Meca";
                var dataBody = Util.GetTemplateVariables();

                StringBuilder message = new StringBuilder();
                message.AppendLine($"Nome completo: {model.FullName}.<br/>");
                message.AppendLine($"Telefone: {model.Phone}.<br/>");
                message.AppendLine($"E-mail: {model.Email}.<br/>");
                message.AppendLine($"Mensagem: {model.Description}.<br/>");

                dataBody.Add("{{ message }}", message.ToString().ReplaceTag(dataBody));
                dataBody.Add("{{ title }}", title);

                var body = _senderMailService.GerateBody("custom", dataBody);

                await _senderMailService.SendMessageEmailAsync(
                    BaseConfig.ApplicationName,
                    contactEmail,
                    body,
                    subject);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Data.Entities.Profile> SyncProfileWithStripe(Data.Entities.Profile profileEntity)
        {
            // bool needsSync = string.IsNullOrEmpty(profileEntity.ExternalId)
            //               || profileEntity.LastSyncStripe < profileEntity.LastUpdate;

            // if (!needsSync) return profileEntity;

            // var customerRequest = profileEntity.MapStripeCustomerRequest();
            // bool hasAccount = !string.IsNullOrEmpty(profileEntity.ExternalId);

            // var result = !hasAccount
            //     ? await _stripeCustomerService.CreateOrGetCustomerAsync(customerRequest)
            //     : await _stripeCustomerService.UpdateCustomerAsync(customerRequest);

            // if (hasAccount && !result.Success)
            // {
            //     CreateNotification(result.ErrorMessage);
            //     return null;
            // }

            // profileEntity.ExternalId = result.Data?.Id ?? profileEntity.ExternalId;
            // profileEntity.LastSyncStripe = DateTimeOffset.Now.ToUnixTimeSeconds();

            // return await _profileRepository.UpdateAsync(profileEntity);
            return profileEntity;
        }
    }
}
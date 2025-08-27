using System;
using System.Globalization;
using System.Linq;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Admin;
using Meca.Domain.ViewModels.Auxiliaries;
using Meca.Domain.ViewModels.Export;
using MongoDB.Bson;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Services.Iugu.Core3.Entity;
using UtilityFramework.Services.Iugu.Core3.Models;
using AutoMapperProfile = AutoMapper.Profile;

namespace Meca.Domain.AutoMapper
{
    public class DomainToViewModelMappingProfile : AutoMapperProfile
    {
        public DomainToViewModelMappingProfile()
        {
            /**
            CreateMap<Entity,ViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            */
            CreateMap<ItemMenuRule, ItemMenuRuleViewModel>().ReverseMap();
            CreateMap<NotificationAux, Notification>().ReverseMap();
            CreateMap<BaseReferenceAux, BaseReferenceAuxViewModel>().ReverseMap();
            CreateMap<BaseReferenceAux, AccessLevelViewModel>().ReverseMap();
            CreateMap<AccessLevel, AccessLevelViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<IuguCreditCard, CreditCardViewModel>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Data.DisplayNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Data.HolderName))
                .ForMember(dest => dest.ExpMonth, opt => opt.MapFrom(src => src.Data.Month))
                .ForMember(dest => dest.ExpYear, opt => opt.MapFrom(src => src.Data.Year))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Data.Brand));
            CreateMap<Bank, BankViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));
            CreateMap<AddressInfoViewModel, InfoAddressViewModel>()
                .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Bairro))
                .ForMember(dest => dest.StateUf, opt => opt.MapFrom(src => src.Uf))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.Logradouro))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Cep.ToString()));
            CreateMap<UserAdministrator, UserAdministratorViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<UserAdministratorAux, UserAdministratorAuxViewModel>().ReverseMap();
            CreateMap<UserAdministratorAux, UserAdministrator>().ReverseMap();
            CreateMap<Profile, ProfileViewModel>()
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo.SetPhotoProfile(src.ProviderId, null, null, null, null, 600)))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Profile, PayerModel>()
               .ForMember(dest => dest.Address, opt => opt.MapFrom(src => Util.SetAddress(src)))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
               .ForMember(dest => dest.PhonePrefix, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Phone) == false ? src.Phone.Substring(0, 2) : "11"))
               .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Phone) == false ? src.Phone.Substring(2) : "20392020"))
               .ForMember(dest => dest.CpfOrCnpj, opt => opt.MapFrom(src => src.Cpf));
            CreateMap<State, StateDefaultViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<City, CityDefaultViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Notification, NotificationViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Data.Entities.WorkshopServices, WorkshopServicesViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Data.Entities.ServicesDefault, ServicesDefaultViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Fees, FeesViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Workshop, WorkshopViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id != null ? src._id.ToString() : null))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Cpf))
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.Cnpj))
                .ForMember(dest => dest.MeiCard, opt => opt.MapFrom(src => src.MeiCard))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.FileDocument, opt => opt.MapFrom(src => src.FileDocument))
                .ForMember(dest => dest.OpeningHours, opt => opt.MapFrom(src => src.OpeningHours))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Login, opt => opt.MapFrom(src => src.Login))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.CityName))
                .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId))
                .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.StateName))
                .ForMember(dest => dest.StateUf, opt => opt.MapFrom(src => src.StateUf))
                .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.StateId))
                .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Neighborhood))
                .ForMember(dest => dest.Complement, opt => opt.MapFrom(src => src.Complement))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null))
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo));
            CreateMap<Object, TokenResponseViewModel>();
            CreateMap<WorkshopAux, WorkshopAuxViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.Cnpj))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.CityName))
                .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId))
                .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.StateName))
                .ForMember(dest => dest.StateUf, opt => opt.MapFrom(src => src.StateUf))
                .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.StateId))
                .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Neighborhood))
                .ForMember(dest => dest.Complement, opt => opt.MapFrom(src => src.Complement))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason));
            CreateMap<WorkshopAux, WorkshopViewModel>().ReverseMap();
            CreateMap<WorkshopAux, Workshop>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.Cnpj))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.CityName))
                .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId))
                .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.StateName))
                .ForMember(dest => dest.StateUf, opt => opt.MapFrom(src => src.StateUf))
                .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.StateId))
                .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Neighborhood))
                .ForMember(dest => dest.Complement, opt => opt.MapFrom(src => src.Complement))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason));
            CreateMap<WorkshopAgendaAux, WorkshopAgendaAuxViewModel>().ReverseMap();
            CreateMap<WorkshopAgenda, WorkshopAgendaViewModel>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ObjectId.Parse(src._id.ToString())));
            CreateMap<Workshop, Meca.Domain.ViewModels.DataBankViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ObjectId.Parse(src._id.ToString())));
            CreateMap<Vehicle, VehicleViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ObjectId.Parse(src._id.ToString())));
            CreateMap<ProfileAux, ProfileAuxViewModel>().ReverseMap();
            CreateMap<ProfileAux, Profile>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<ProfileAuxViewModel, ProfileAux>().ReverseMap();
            CreateMap<Scheduling, SchedulingViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.Workshop, opt => opt.MapFrom(src => src.Workshop))
                .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                .ForMember(dest => dest.WorkshopServices, opt => opt.MapFrom(src => src.WorkshopServices));
            CreateMap<WorkshopServicesAux, WorkshopServicesAuxViewModel>().ReverseMap();
            CreateMap<WorkshopServicesAux, WorkshopServicesViewModel>().ReverseMap();
            CreateMap<WorkshopServicesAux, WorkshopServicesAuxViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                .ForMember(dest => dest.MinTimeScheduling, opt => opt.MapFrom(src => src.MinTimeScheduling))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<VehicleAux, VehicleAuxViewModel>().ReverseMap();
            CreateMap<SchedulingHistory, SchedulingHistoryViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<FinancialHistory, FinancialHistoryExportViewModel>()
                .ForMember(dest => dest.ProfileName, opt => opt.MapFrom(src => src.Profile.FullName))
                .ForMember(dest => dest.VehiclePlate, opt => opt.MapFrom(src => src.Vehicle.Plate))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.WorkshopServices.Select(x => x.Service.Name).ToList()))
                .ForMember(dest => dest.WorkshopName, opt => opt.MapFrom(src => src.Workshop.FullName))
                .ForMember(dest => dest.WorkshopCnpj, opt => opt.MapFrom(src => src.Workshop.Cnpj))
                .ForMember(dest => dest.WorkshopResponsibleName, opt => opt.MapFrom(src => src.Workshop.FullName))
                .ForMember(dest => dest.LastUpdate, opt => opt.MapFrom(src => src.LastUpdate.MapUnixTime("dd/MM/yyyy HH:mm", "-")))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value.AroundABNT(2).ToString("N2", new CultureInfo("en-US"))))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.GetEnumMemberValue()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus.GetEnumMemberValue()))
                .ForMember(dest => dest.RefundDate, opt => opt.MapFrom(src => src.RefundDate.MapUnixTime("dd/MM/yyyy HH:mm", "-")))
                .ForMember(dest => dest.ReversedValue, opt => opt.MapFrom(src => src.ReversedValue.HasValue ? src.ReversedValue.Value.AroundABNT(2).ToString("N2", new CultureInfo("en-US")) : null))
                .ForMember(dest => dest.AmountPaidForTheWorkshop, opt => opt.MapFrom(src => src.NetValue.HasValue ? src.NetValue.Value.AroundABNT(2).ToString("N2", new CultureInfo("en-US")) : null))
                .ForMember(dest => dest.AmountPaidToMecca, opt => opt.MapFrom(src => src.PlatformValue.HasValue ? src.PlatformValue.Value.AroundABNT(2).ToString("N2", new CultureInfo("en-US")) : null))
                .ForMember(dest => dest.ProcessingValue, opt => opt.MapFrom(src => src.ProcessingValue.HasValue ? src.ProcessingValue.Value.AroundABNT(2).ToString("N2", new CultureInfo("en-US")) : null));
            CreateMap<FinancialHistory, FinancialHistoryViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Rating, RatingViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Rating, RatingViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Vehicle, VehicleAux>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Faq, FaqViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
        }
    }
}

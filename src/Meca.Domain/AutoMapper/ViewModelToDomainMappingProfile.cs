using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Admin;
using Meca.Domain.ViewModels.Auxiliaries;
using MongoDB.Bson;
using AutoMapperProfile = AutoMapper.Profile;

namespace Meca.Domain.AutoMapper
{
    public class ViewModelToDomainMappingProfile : AutoMapperProfile
    {
        public ViewModelToDomainMappingProfile()
        {
            /*EXEMPLE*/
            //CreateMap<ViewModel, Entity>()
            //    .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<AccessLevelViewModel, BaseReferenceAux>().ReverseMap();
            CreateMap<AccessLevelViewModel, AccessLevel>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<UserAdministratorViewModel, UserAdministrator>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ProfileRegisterViewModel, Profile>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<Profile, Notification>()
                .ForMember(dest => dest._id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Disabled, opt => opt.Ignore())
                .ForMember(dest => dest.DataBlocked, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdate, opt => opt.Ignore())
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.ReferenceName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.TypeReference, opt => opt.MapFrom(src => TypeProfile.Profile));
            CreateMap<Profile, NotificationAux>()
                .ForMember(dest => dest.TypeReference, opt => opt.MapFrom(src => TypeProfile.Profile))
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.ReferenceName, opt => opt.MapFrom(src => src.FullName));
            CreateMap<Workshop, Notification>()
                .ForMember(dest => dest._id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Disabled, opt => opt.Ignore())
                .ForMember(dest => dest.DataBlocked, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdate, opt => opt.Ignore())
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.ReferenceName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.TypeReference, opt => opt.MapFrom(src => TypeProfile.Workshop));
            CreateMap<Workshop, NotificationAux>()
                .ForMember(dest => dest.TypeReference, opt => opt.MapFrom(src => TypeProfile.Workshop))
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.ReferenceName, opt => opt.MapFrom(src => src.FullName));
            CreateMap<UserAdministrator, Notification>()
                .ForMember(dest => dest._id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Disabled, opt => opt.Ignore())
                .ForMember(dest => dest.DataBlocked, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdate, opt => opt.Ignore())
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.ReferenceName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TypeReference, opt => opt.MapFrom(src => TypeProfile.UserAdministrator));
            CreateMap<UserAdministrator, NotificationAux>()
                .ForMember(dest => dest.TypeReference, opt => opt.MapFrom(src => TypeProfile.UserAdministrator))
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.ReferenceName, opt => opt.MapFrom(src => src.Name));
            CreateMap<WorkshopServicesViewModel, Data.Entities.WorkshopServices>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ServicesDefaultViewModel, Data.Entities.ServicesDefault>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FeesViewModel, Fees>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<WorkshopViewModel, Workshop>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<WorkshopRegisterViewModel, Workshop>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<DataBankViewModel, Workshop>()
                .ForMember(dest => dest._id, opt => opt.Ignore()) // Ignorar _id para evitar sobrescrever
                .ForMember(dest => dest.HasDataBank, opt => opt.MapFrom(src => src.HasDataBank))
                .ForMember(dest => dest.AccountableName, opt => opt.MapFrom(src => src.AccountableName))
                .ForMember(dest => dest.AccountableCpf, opt => opt.MapFrom(src => src.AccountableCpf))
                .ForMember(dest => dest.BankAccount, opt => opt.MapFrom(src => src.BankAccount))
                .ForMember(dest => dest.BankAgency, opt => opt.MapFrom(src => src.BankAgency))
                .ForMember(dest => dest.Bank, opt => opt.MapFrom(src => src.Bank))
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.BankName))
                .ForMember(dest => dest.BankCnpj, opt => opt.MapFrom(src => src.BankCnpj))
                .ForMember(dest => dest.TypeAccount, opt => opt.MapFrom(src => src.TypeAccount))
                .ForMember(dest => dest.PersonType, opt => opt.MapFrom(src => src.PersonType))
                .ForMember(dest => dest.DataBankStatus, opt => opt.MapFrom(src => src.DataBankStatus));
            CreateMap<WorkshopAgendaViewModel, WorkshopAgenda>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<VehicleViewModel, Vehicle>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<SchedulingViewModel, Scheduling>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<SendBudgetViewModel, Scheduling>().ReverseMap();
            CreateMap<SchedulingHistoryViewModel, SchedulingHistory>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<RatingViewModel, Rating>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FinancialHistoryViewModel, FinancialHistory>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FinancialHistoryViewModel, WorkshopServicesAuxViewModel>().ReverseMap();
            CreateMap<WorkshopServicesAuxViewModel, WorkshopServicesAux>().ReverseMap();
            CreateMap<WorkshopServicesAuxViewModel, WorkshopServicesAux>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                .ForMember(dest => dest.MinTimeScheduling, opt => opt.MapFrom(src => src.MinTimeScheduling))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<FinancialHistoryViewModel, WorkshopServices>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FaqViewModel, Faq>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<BudgetServicesAuxViewModel, BudgetServicesAux>().ReverseMap();
            CreateMap<WorkshopAux, WorkshopAuxViewModel>().ReverseMap();
            CreateMap<WorkshopAux, WorkshopViewModel>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)))
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
            CreateMap<WorkshopAuxViewModel, WorkshopAux>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
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
        }
    }
}

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
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)))
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
            CreateMap<FinancialHistoryViewModel, WorkshopServices>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FaqViewModel, Faq>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<BudgetServicesAuxViewModel, BudgetServicesAux>().ReverseMap();
        }
    }
}

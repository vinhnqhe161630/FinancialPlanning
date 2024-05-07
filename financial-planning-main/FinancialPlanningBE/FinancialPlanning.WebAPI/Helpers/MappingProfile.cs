using System.ComponentModel;
using AutoMapper;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.WebAPI.Models.Department;
using FinancialPlanning.WebAPI.Models.Expense;
using FinancialPlanning.WebAPI.Models.Plan;
using FinancialPlanning.WebAPI.Models.Position;
using FinancialPlanning.WebAPI.Models.Report;
using FinancialPlanning.WebAPI.Models.Role;
using FinancialPlanning.WebAPI.Models.Term;
using FinancialPlanning.WebAPI.Models.User;

namespace FinancialPlanning.WebAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Term, TermListModel>()
                .ForMember(dest => dest.EndDate,
                    opt => opt.MapFrom(src => src.StartDate.AddMonths(src.Duration)))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => GetEnumDescription(src.Status)));
            CreateMap<Term, TermViewModel>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => GetEnumDescription(src.Status)))
                ;
            CreateMap<CreateTermModel, Term>();

            CreateMap<LoginModel, User>();
            CreateMap<ResetPasswordModel, User>();

            // map report to  ReportViewModel
            CreateMap<Report, ReportViewModel>()
           .ForMember(dest => dest.TermName, opt => opt.MapFrom(src => src.Term.TermName))
           .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartmentName))
           .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.GetMaxVersion()))
           .ForMember(dest => dest.ReportDureDate, opt => opt.MapFrom(src => src.Term.ReportDueDate));
    

            // map reportViewModel to  Report
            CreateMap<ReportViewModel, Report>();
            // map reportVersion to reportVersionModel
            CreateMap<ReportVersion, ReportVersionModel>()
            .ForMember(dest => dest.UploadedBy, opt => opt.MapFrom(src => src.User.Username));

            //Map department
            CreateMap<Department, DepartmentViewModel>();
            CreateMap<DepartmentViewModel, Department>();

            CreateMap<ReportViewModel, Report>();

            // Map plan 

            CreateMap<PlanListModel, Plan>();

            CreateMap<Plan, PlanViewModel>()
                .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.PlanName))
                .ForMember(dest => dest.Term, opt => opt.MapFrom(src => src.Term.TermName))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.DepartmentName))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.GetMaxVersion()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.Term.PlanDueDate));
            CreateMap<Term, SelectTermModel>();

            CreateMap<RoleViewModel, Role>().ReverseMap();
            CreateMap<PositionViewModel, Position>().ReverseMap();
            //map User to userModel
            CreateMap<User, UserModel>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartmentName))
                .ForMember(dest => dest.PositionName, opt => opt.MapFrom(src => src.Position.PositionName))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));


            //map User to AddUserModel
            CreateMap<User, AddUserModel>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.DOB, opt => opt.MapFrom(src => src.DOB))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.PositionId, opt => opt.MapFrom(src => src.PositionId))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)).ReverseMap();

            // map planVersion to planVersionModel
            CreateMap<PlanVersion, PlanVersionModel>()
            .ForMember(dest => dest.UploadedBy, opt => opt.MapFrom(src => src.User.Username));

            // map Expense
            CreateMap<Expense, ExpenseStatusModel>();
            CreateMap<ExpenseStatusModel, Expense>();
        }

        private static string GetEnumDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var descriptionAttributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (descriptionAttributes == null || descriptionAttributes.Length == 0)
                return value.ToString();

            return descriptionAttributes[0].Description;
        }
    }
}
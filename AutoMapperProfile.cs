using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using financing_api.Dtos.User;
using financing_api.Dtos.Transaction;
using financing_api.Dtos.Account;
using financing_api.Dtos.Category;
using financing_api.Dtos.Frequency;
using financing_api.Dtos.UserSetting;
using financing_api.Dtos.ManagedBill;

namespace financing_api
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, LoadUserDto>();
            CreateMap<AddRecurringDto, Recurring>();
            CreateMap<Recurring, RecurringDto>();
            CreateMap<RecurringDto, Recurring>();
            CreateMap<UpdateRecurringDto, Recurring>();
            CreateMap<Recurring, GetRecurringDto>();
            CreateMap<TransactionDto, Transaction>();
            CreateMap<Transaction, TransactionDto>();
            CreateMap<AccountDto, Account>();
            CreateMap<Account, AccountDto>();
            CreateMap<CategoryDto, Category>();
            CreateMap<Category, CategoryDto>();
            CreateMap<AddCategoryDto, Category>();
            CreateMap<Category, AddCategoryDto>();
            CreateMap<FrequencyDto, Frequency>();
            CreateMap<Frequency, FrequencyDto>();
            CreateMap<AddFrequencyDto, Frequency>();
            CreateMap<Frequency, AddFrequencyDto>();
            CreateMap<SettingsDto, UserSettings>();
            CreateMap<UserSettings, SettingsDto>();
            CreateMap<ManagedBill, BillDto>();
            CreateMap<UpdateBillDto, ManagedBill>();
            CreateMap<AddBillDto, ManagedBill>();
        }
    }
}

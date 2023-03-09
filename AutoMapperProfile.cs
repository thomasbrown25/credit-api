using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using financing_api.Dtos.User;
using financing_api.Dtos.Character;
using financing_api.Dtos.Skill;
using financing_api.Dtos.Weapon;
using financing_api.Dtos.Transaction;
using financing_api.Dtos.Account;
using financing_api.Dtos.Category;

namespace financing_api
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterDto>();
            CreateMap<AddCharacterDto, Character>();
            CreateMap<UpdateCharacterDto, Character>();
            CreateMap<Weapon, GetWeaponDto>();
            CreateMap<Skill, GetSkillDto>();
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
        }
    }
}

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
            CreateMap<Recurring, financing_api.Dtos.Transaction.GetRecurringDto>();
        }
    }
}

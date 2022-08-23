using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_api_netcore_project.Dtos.Character;
using web_api_netcore_project.Dtos.Weapon;

namespace web_api_netcore_project.Services.WeaponService
{
    public interface IWeaponService
    {
        Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWeapon);
    }
}
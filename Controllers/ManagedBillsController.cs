using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.ManagedBill;
using financing_api.Services.ManagedBillsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagedBillsController : ControllerBase
    {
        private readonly IManagedBillsService _managedBillsService;
        public ManagedBillsController(IManagedBillsService managedBillsService)
        {
            _managedBillsService = managedBillsService;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<ActionResult<ServiceResponse<GetBillsDto>>> GetBills()
        {
            var response = await _managedBillsService.GetBills();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("")]
        public async Task<ActionResult<ServiceResponse<GetBillsDto>>> AddBill(AddBillDto bill)
        {
            var response = await _managedBillsService.AddBill(bill);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("{id}")]
        public async Task<ActionResult<ServiceResponse<GetBillsDto>>> UpdateBill(UpdateBillDto updatedBill)
        {
            var response = await _managedBillsService.UpdateBill(updatedBill);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<GetBillsDto>>> DeleteBill(int id)
        {
            var response = await _managedBillsService.DeleteBill(id);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
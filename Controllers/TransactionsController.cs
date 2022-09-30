using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Transaction;
using financing_api.Services.TransactionsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionsService _transactionsService;

        public TransactionsController(ITransactionsService transactionService)
        {
            _transactionsService = transactionService;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<ActionResult<ServiceResponse<string>>> GetTransactions()
        {
            var response = await _transactionsService.GetTransactions();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("recent")]
        public async Task<ActionResult<ServiceResponse<string>>> GetRecentTransactions(uint count)
        {
            var response = await _transactionsService.GetRecentTransactions(count);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("current-month")]
        public async Task<
            ActionResult<ServiceResponse<GetTransactionsDto>>
        > GetCurrentMonthTransactions()
        {
            var response = await _transactionsService.GetCurrentMonthTransactions();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("current-spend-month")] // Gets all transactions for all accounts
        public async Task<ActionResult<ServiceResponse<decimal>>> GetCurrentSpendForMonth()
        {
            var response = await _transactionsService.GetCurrentSpendForMonth();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}

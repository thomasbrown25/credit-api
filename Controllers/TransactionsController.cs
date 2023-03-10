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
        [HttpPost("refresh")]
        public async Task<ActionResult<ServiceResponse<GetTransactionsDto>>> RefreshTransactions()
        {
            var response = await _transactionsService.RefreshTransactions();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("recurring")]
        public async Task<ActionResult<ServiceResponse<GetRecurringDto>>> GetRecurringTransactions()
        {
            var response = await _transactionsService.GetRecurringTransactions();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("income/{incomeId}")]
        public async Task<ActionResult<ServiceResponse<GetRecurringDto>>> DeleteIncome(string incomeId)
        {
            var response = await _transactionsService.DeleteIncome(incomeId);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("income/activate/{incomeId}")]
        public async Task<ActionResult<ServiceResponse<GetRecurringDto>>> SetIncomeActive(string incomeId, UpdateRecurringDto recurringDto)
        {
            var response = await _transactionsService.SetIncomeActive(incomeId, recurringDto);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("expenses")]
        public async Task<ActionResult<ServiceResponse<GetRecurringDto>>> GetExpenses()
        {
            var response = await _transactionsService.GetExpenses();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("account-transactions/{accountId}")]
        public async Task<ActionResult<ServiceResponse<GetRecurringDto>>> GetAccountTransactions(string accountId)
        {
            var response = await _transactionsService.GetAccountTransactions(accountId);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("recurring")]
        public async Task<ActionResult<ServiceResponse<List<RecurringDto>>>> AddRecurringTransaction(AddRecurringDto newRecurring)
        {
            var response = await _transactionsService.AddRecurringTransaction(newRecurring);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("recurring/{transactionId}")]
        public async Task<ActionResult<ServiceResponse<GetRecurringDto>>> UpdateRecurringTransaction(UpdateRecurringDto updatedRecurring)
        {
            var response = await _transactionsService.UpdateRecurringTransaction(updatedRecurring);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("recurring/refresh")]
        public async Task<ActionResult<ServiceResponse<List<RecurringDto>>>> RefreshRecurringTransactions()
        {
            var response = await _transactionsService.RefreshRecurringTransactions();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using System.Security.Claims;
using financing_api.Dtos.Transaction;
using financing_api.Logging;
using Going.Plaid;
using AutoMapper;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Options;
using financing_api.Shared;
using financing_api.Dtos.Account;
using financing_api.Utils;
using System.Collections;
using financing_api.ApiHelper;
using financing_api.Dtos.Category;

namespace financing_api.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IAPI _api;

        public CategoryService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IAPI api
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _api = api;
        }

        public async Task<ServiceResponse<GetCategoryDto>> GetCategories()
        {
            var response = new ServiceResponse<GetCategoryDto>();

            try
            {
                response.Data = new GetCategoryDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbCategories = await _context.Categories
                                   .OrderBy(c => c.Name)
                                   .ToListAsync();

                response.Data.Categories = dbCategories.Select(c => _mapper.Map<CategoryDto>(c)).ToList();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Get Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto category)
        {
            var response = new ServiceResponse<GetCategoryDto>();

            try
            {
                response.Data = new GetCategoryDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                Category newCategory = _mapper.Map<Category>(category);

                _context.Categories.Add(newCategory);

                await _context.SaveChangesAsync();

                var dbCategories = await _context.Categories
                                   .OrderBy(c => c.Name)
                                   .ToListAsync();

                response.Data.Categories = dbCategories.Select(c => _mapper.Map<CategoryDto>(c)).ToList();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Get Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<GetCategoryDto>> RefreshCategories()
        {
            var response = new ServiceResponse<GetCategoryDto>();

            try
            {
                response.Data = new GetCategoryDto();
                response.Data.Categories = new List<CategoryDto>();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var result = await _api.GetTransactionsRequest(user);

                foreach (var transaction in result.Transactions)
                {
                    var category = transaction.Category?[0];
                    var dbCategory = await _context.Categories
                                        .FirstOrDefaultAsync(c => c.Name.ToLower() == category);

                    if (dbCategory is null)
                    {
                        var categoryDto = new CategoryDto();

                        categoryDto.Name = category;

                        response.Data.Categories.Add(categoryDto);
                        Category categoryDb = _mapper.Map<Category>(categoryDto);
                        _context.Categories.Add(categoryDb);
                        await _context.SaveChangesAsync();
                    }
                }

                var dbCategories = await _context.Categories
                                   .OrderBy(c => c.Name)
                                   .ToListAsync();

                response.Data.Categories = dbCategories.Select(c => _mapper.Map<CategoryDto>(c)).ToList();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Get Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }
    }
}
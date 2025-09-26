using AutoMapper;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Validators;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Interfaces.Repositories;

namespace MoneyTracker.Application.Services;

/// <summary>
/// Application service for categories.
/// </summary>
public class CategoryAppService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly CategoryValidator _validator;

    public CategoryAppService(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        CategoryValidator validator)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _validator = validator;
    }

    /// <summary>
    /// Retrieves all active categories.
    /// </summary>
    public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync();
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    /// <summary>
    /// Retrieves all categories with statistics.
    /// </summary>
    public async Task<List<CategoryDto>> GetCategoriesWithStatsAsync()
    {
        var categories = await _categoryRepository.GetCategoriesWithTransactionsAsync();
        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);

        // Calculate additional statistics
        foreach (var categoryDto in categoryDtos)
        {
            var transactions = await _transactionRepository.GetByCategoryAsync(categoryDto.Id);
            categoryDto.TransactionCount = transactions.Count;
            categoryDto.TotalAmount = transactions.Sum(t => Math.Abs(t.Amount.Amount));
            categoryDto.CanDelete = transactions.Count == 0;
        }

        return categoryDtos.OrderByDescending(c => c.TotalAmount).ToList();
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    public async Task<(bool Success, CategoryDto? Category, List<string> Errors)> CreateCategoryAsync(CategoryDto categoryDto)
    {
        // 1. Validate the input
        var validationResult = await _validator.ValidateAsync(categoryDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return (false, null, errors);
        }

        try
        {
            // 2. Convert to an entity
            var category = _mapper.Map<Category>(categoryDto);

            // 3. Validate domain rules
            if (!category.IsValid(out var domainErrors))
            {
                return (false, null, domainErrors);
            }

            // 4. Persist the entity
            var savedCategory = await _categoryRepository.AddAsync(category);
            var resultDto = _mapper.Map<CategoryDto>(savedCategory);

            return (true, resultDto, new List<string>());
        }
        catch (Exception ex)
        {
            var errorMessage = "Error creating the category: " + ex.Message;
            return (false, null, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Initializes default categories if they do not exist.
    /// </summary>
    public async Task<int> InitializeDefaultCategoriesAsync()
    {
        try
        {
            var existingCategories = await _categoryRepository.GetAllAsync();
            if (existingCategories.Any())
            {
                return 0; // Categories already exist
            }

            var defaultCategories = Category.GetDefaultCategories();
            int created = 0;

            foreach (var category in defaultCategories)
            {
                await _categoryRepository.AddAsync(category);
                created++;
            }

            return created;
        }
        catch (Exception ex)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Error initializing categories: {ex}");
            return 0;
        }
    }

    /// <summary>
    /// Deletes a category if it has no transactions.
    /// </summary>
    public async Task<(bool Success, List<string> Errors)> DeleteCategoryAsync(int id)
    {
        try
        {
            // 1. Ensure it exists
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return (false, new List<string> { "The category does not exist" });
            }

            // 2. Ensure it does not have transactions
            var hasTransactions = await _categoryRepository.HasTransactionsAsync(id);
            if (hasTransactions)
            {
                return (false, new List<string> { "Cannot delete a category that has transactions" });
            }

            // 3. Delete
            var deleted = await _categoryRepository.DeleteAsync(id);

            return deleted
                ? (true, new List<string>())
                : (false, new List<string> { "Error deleting the category" });
        }
        catch (Exception ex)
        {
            var errorMessage = "Error deleting the category: " + ex.Message;
            return (false, new List<string> { errorMessage });
        }
    }
}
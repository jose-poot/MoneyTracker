using AutoMapper;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Validators;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Interfaces.Repositories;

namespace MoneyTracker.Application.Services;

/// <summary>
/// Servicio de aplicación para categorías
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
    /// Obtiene todas las categorías activas
    /// </summary>
    public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync();
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    /// <summary>
    /// Obtiene todas las categorías con estadísticas
    /// </summary>
    public async Task<List<CategoryDto>> GetCategoriesWithStatsAsync()
    {
        var categories = await _categoryRepository.GetCategoriesWithTransactionsAsync();
        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);

        // Calcular estadísticas adicionales
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
    /// Crea una nueva categoría
    /// </summary>
    public async Task<(bool Success, CategoryDto? Category, List<string> Errors)> CreateCategoryAsync(CategoryDto categoryDto)
    {
        // 1. Validar
        var validationResult = await _validator.ValidateAsync(categoryDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return (false, null, errors);
        }

        try
        {
            // 2. Convertir a entidad
            var category = _mapper.Map<Category>(categoryDto);

            // 3. Validar reglas de dominio
            if (!category.IsValid(out var domainErrors))
            {
                return (false, null, domainErrors);
            }

            // 4. Guardar
            var savedCategory = await _categoryRepository.AddAsync(category);
            var resultDto = _mapper.Map<CategoryDto>(savedCategory);

            return (true, resultDto, new List<string>());
        }
        catch (Exception ex)
        {
            var errorMessage = "Error al crear la categoría: " + ex.Message;
            return (false, null, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Inicializa categorías predeterminadas si no existen
    /// </summary>
    public async Task<int> InitializeDefaultCategoriesAsync()
    {
        try
        {
            var existingCategories = await _categoryRepository.GetAllAsync();
            if (existingCategories.Any())
            {
                return 0; // Ya hay categorías
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
    /// Elimina una categoría si no tiene transacciones
    /// </summary>
    public async Task<(bool Success, List<string> Errors)> DeleteCategoryAsync(int id)
    {
        try
        {
            // 1. Verificar que existe
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return (false, new List<string> { "La categoría no existe" });
            }

            // 2. Verificar que no tiene transacciones
            var hasTransactions = await _categoryRepository.HasTransactionsAsync(id);
            if (hasTransactions)
            {
                return (false, new List<string> { "No se puede eliminar una categoría que tiene transacciones" });
            }

            // 3. Eliminar
            var deleted = await _categoryRepository.DeleteAsync(id);

            return deleted
                ? (true, new List<string>())
                : (false, new List<string> { "Error al eliminar la categoría" });
        }
        catch (Exception ex)
        {
            var errorMessage = "Error al eliminar la categoría: " + ex.Message;
            return (false, new List<string> { errorMessage });
        }
    }
}
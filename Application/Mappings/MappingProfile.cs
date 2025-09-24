using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.ValueObjects;
using static Android.Provider.ContactsContract;
using AutoMapper;
using Profile = AutoMapper.Profile;

namespace MoneyTracker.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper que define cómo convertir entre entidades y DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ConfigureTransactionMappings();
        ConfigureCategoryMappings();
        ConfigureUserMappings();
    }

    private void ConfigureTransactionMappings()
    {
        // Transaction Entity → TransactionDto
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color))
            .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category.Icon))
            .ForMember(dest => dest.IsValid, opt => opt.Ignore())
            .ForMember(dest => dest.ValidationErrors, opt => opt.Ignore());

        // CreateTransactionDto → Transaction Entity
        CreateMap<CreateTransactionDto, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src =>
                new Money(src.Amount, src.Currency)))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Para actualización: TransactionDto → Transaction
        CreateMap<TransactionDto, Transaction>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src =>
                new Money(src.Amount, src.Currency)))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }

    private void ConfigureCategoryMappings()
    {
        // Category Entity → CategoryDto
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.TransactionCount, opt => opt.MapFrom(src => src.Transactions.Count))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src =>
                src.Transactions.Sum(t => Math.Abs(t.Amount.Amount))))
            .ForMember(dest => dest.IsSelected, opt => opt.Ignore())
            .ForMember(dest => dest.CanDelete, opt => opt.MapFrom(src => src.Transactions.Count == 0));

        // CategoryDto → Category Entity (para crear/actualizar)
        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id > 0))
            .ForMember(dest => dest.Transactions, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }

    private void ConfigureUserMappings()
    {
        // User mappings (para futuro)
        CreateMap<User, UserSettingsDto>();
        CreateMap<UserSettingsDto, User>();
    }
}
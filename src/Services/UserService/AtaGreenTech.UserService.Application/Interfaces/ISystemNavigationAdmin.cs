using AtaGreenTech.UserService.Application.Features.System.DTOs;

namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Master DB dil / modül / menü yönetim işlemleri.</summary>
public interface ISystemNavigationAdmin
{
    Task<IReadOnlyList<SystemLanguageDto>> GetLanguagesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<SystemModuleDto>> GetModulesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemMenuListItemDto>> GetMenusAsync(CancellationToken cancellationToken = default);

    Task<SystemMenuDetailDto?> GetMenuAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateMenuAsync(CreateSystemMenuRequest request, CancellationToken cancellationToken = default);

    Task UpdateMenuAsync(Guid id, UpdateSystemMenuRequest request, CancellationToken cancellationToken = default);

    Task PatchMenuAsync(Guid id, PatchSystemMenuRequest request, CancellationToken cancellationToken = default);

    Task UpdateModuleAsync(Guid id, UpdateSystemModuleRequest request, CancellationToken cancellationToken = default);
}

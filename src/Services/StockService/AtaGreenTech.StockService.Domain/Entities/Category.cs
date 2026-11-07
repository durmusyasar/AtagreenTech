using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

public class Category : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    private readonly List<Category> _subCategories = [];
    private readonly List<Product> _products = [];

    protected Category()
    {
        Name = null!;
    }

    public Category(string name, string? description, Guid? parentCategoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = NormalizeDescription(description);
        ParentCategoryId = parentCategoryId;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Üst kategori; kök kategorilerde <see langword="null"/>.
    /// </summary>
    public Guid? ParentCategoryId { get; private set; }

    public Category? ParentCategory { get; private set; }

    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    /// <remarks>
    /// Yalnızca kategorinin kendisini üst kategori yapması engellenir; daha derin döngüler (A → B → A) hiyerarşinin
    /// okunmasını gerektirdiği için application katmanında kontrol edilir.
    /// </remarks>
    public void Update(string name, string? description, Guid? parentCategoryId, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        CheckRule.ThrowIf(
            new CategoryCannotBeItsOwnParentRule(Id, parentCategoryId),
            CatalogErrors.CategoryCannotBeItsOwnParent,
            Id
        );

        Name = name.Trim();
        Description = NormalizeDescription(description);
        ParentCategoryId = parentCategoryId;
        IsActive = isActive;
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}

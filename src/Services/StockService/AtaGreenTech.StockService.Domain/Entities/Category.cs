using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Localization;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Hiyerarşik ürün kategorisi. Dilden bağımsız kodu (ör. SEED, FERT) kategoriyi tanımlar; adı ve açıklaması dillere
/// göre <see cref="Translations"/>'da tutulur.
/// </summary>
public class Category : BaseEntity
{
    public const int CodeMaxLength = 50;

    private readonly List<CategoryTranslation> _translations = [];
    private readonly List<Category> _subCategories = [];
    private readonly List<Product> _products = [];

    protected Category()
    {
        Code = null!;
    }

    /// <param name="defaultLanguage">Çeviriler arasında bulunması zorunlu olan sistem dili.</param>
    public Category(
        string code,
        IReadOnlyCollection<CategoryTranslation> translations,
        string defaultLanguage,
        Guid? parentCategoryId = null
    )
        : this()
    {
        SetCode(code);
        SetTranslations(translations, defaultLanguage);
        ParentCategoryId = parentCategoryId;
    }

    /// <summary>
    /// Silinmemiş kategoriler arasında benzersiz kod; büyük harfe çevrilerek saklanır.
    /// </summary>
    public string Code { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<CategoryTranslation> Translations => _translations.AsReadOnly();

    /// <summary>
    /// Üst kategori; kök kategorilerde <see langword="null"/>.
    /// </summary>
    public Guid? ParentCategoryId { get; private set; }

    public Category? ParentCategory { get; private set; }

    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    /// <summary>
    /// Kodu, çevirileri, üst kategoriyi ve aktifliği günceller. Var olan dillerin metni yerinde güncellenir, yeni
    /// diller eklenir, listede olmayan diller silinir.
    /// </summary>
    /// <remarks>
    /// Yalnızca kategorinin kendisini üst kategori yapması engellenir; daha derin döngüler (A → B → A) hiyerarşinin
    /// okunmasını gerektirdiği için application katmanında kontrol edilir.
    /// </remarks>
    public void Update(
        string code,
        IReadOnlyCollection<CategoryTranslation> translations,
        string defaultLanguage,
        Guid? parentCategoryId,
        bool isActive
    )
    {
        CheckRule.ThrowIf(
            new CategoryCannotBeItsOwnParentRule(Id, parentCategoryId),
            CatalogErrors.CategoryCannotBeItsOwnParent,
            Id
        );

        SetCode(code);
        SetTranslations(translations, defaultLanguage);
        ParentCategoryId = parentCategoryId;
        IsActive = isActive;
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private void SetCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = NormalizeCode(code);
    }

    private void SetTranslations(IReadOnlyCollection<CategoryTranslation> translations, string defaultLanguage) =>
        TranslationCollection.Replace(
            _translations,
            translations,
            defaultLanguage,
            (existing, updated) => existing.SetText(updated.Name, updated.Description)
        );
}

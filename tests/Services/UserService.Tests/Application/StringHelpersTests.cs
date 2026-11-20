using AtaGreenTech.UserService.Application.Common;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class StringHelpersTests
{
    [Fact]
    public void FirstNonEmpty_returns_first_non_whitespace_trimmed()
    {
        Assert.Equal("admin", StringHelpers.FirstNonEmpty(null, "  ", " admin ", "other"));
    }

    [Fact]
    public void FirstNonEmpty_returns_null_when_all_empty()
    {
        Assert.Null(StringHelpers.FirstNonEmpty(null, "", "   "));
        Assert.Null(StringHelpers.FirstNonEmpty(Array.Empty<string?>()));
    }

    [Fact]
    public void FirstNonEmpty_enumerable_skips_blank_values()
    {
        IEnumerable<string?> values = ["", "  ", "Stok", "Stock"];
        Assert.Equal("Stok", StringHelpers.FirstNonEmpty(values));
    }
}

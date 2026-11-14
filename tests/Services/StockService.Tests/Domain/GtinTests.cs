using AtaGreenTech.StockService.Domain.Pricing;

namespace AtaGreenTech.StockService.Tests.Domain;

public class GtinTests
{
    [Theory]
    [InlineData("96385074")]
    [InlineData("036000291452")]
    [InlineData("4006381333931")]
    [InlineData("8690504000013")]
    [InlineData("10614141000415")]
    [InlineData(" 4006381333931 ")]
    public void Accepts_valid_gs1_barcodes(string barcode)
    {
        Assert.True(Gtin.IsValid(barcode));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("4006381333932")]
    [InlineData("1234567")]
    [InlineData("400638133393")]
    [InlineData("40063813339311")]
    [InlineData("400638133393A")]
    [InlineData("4006381 33931")]
    public void Rejects_invalid_barcodes(string? barcode)
    {
        Assert.False(Gtin.IsValid(barcode));
    }

    [Fact]
    public void Computes_check_digit_from_the_right()
    {
        Assert.Equal(1, Gtin.CheckDigit("400638133393"));
        Assert.Equal(4, Gtin.CheckDigit("9638507"));
    }
}

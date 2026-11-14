using Microsoft.Extensions.Configuration;

namespace HlidacStatu.ParlamentImport.Tests;

public sealed class ClubLabelerAndOptionsTests
{
    private static readonly ClubLabeler Labeler = new(TestData.Options);

    [Theory]
    [InlineData("Poslanecký klub ODS", "ODS", "ODS")]
    [InlineData("Poslanecký klub ANO 2011", "ANO2011", "ANO 2011")]
    [InlineData("Poslanecký klub Starostové a nezávislí", "STAN", "STAN")]
    [InlineData("Poslanecký klub Svoboda a přímá demokracie", "SPD", "SPD")]
    [InlineData("Senátorský klub ODS a TOP 09", null, "ODS a TOP 09")]
    [InlineData("Senátorský klub Starostové a nezávislí", null, "Starostové a nezávislí")]
    [InlineData("Senátoři nezařazení do klubu", null, "nezařazení")]
    [InlineData("Nezařazení", "Nezařaz", "nezařazení")]
    public void Label_ClubName_ReturnsShortLabel(string name, string? abbreviation, string expected)
    {
        Assert.Equal(expected, Labeler.Label(name, abbreviation));
    }

    [Fact]
    public void Load_MissingKeys_ThrowsListingThem()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ParlamentImport:EpCountryCode"] = "CZ" })
            .Build();

        var error = Assert.Throws<InvalidOperationException>(() => ImportOptions.Load(configuration));

        Assert.Contains("ParlamentImport:PspOpenDataZipUrl", error.Message, StringComparison.Ordinal);
        Assert.Contains("ParlamentImport:MinimumEpMembers", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("ParlamentImport:EpCountryCode", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_TrackedAppSettings_IsValid()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var configuration = new ConfigurationBuilder().AddJsonFile(path).Build();

        var options = ImportOptions.Load(configuration);

        Assert.Equal("CZ", options.EpCountryCode);
        Assert.EndsWith("/", options.EpApiBaseUrl, StringComparison.Ordinal);
    }
}

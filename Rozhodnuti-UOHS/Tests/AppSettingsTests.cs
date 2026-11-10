using System;
using System.IO;
using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class AppSettingsTests
{
    const string ValidJson = """
        {
          "Uohs": {
            "ListingUrl": "https://uohs.gov.cz/cs/verejne-zakazky/sbirky-rozhodnuti.html",
            "ListingPageUrlTemplate": "https://uohs.gov.cz/cs/verejne-zakazky/sbirky-rozhodnuti/{0}.html",
            "AreaFilterFormField": "odbor",
            "DetailUrlTemplate": "http://www.uohs.cz/cs/verejne-zakazky/sbirky-rozhodnuti/detail-{0}.html",
            "LinkBaseUrl": "http://www.uohs.cz",
            "HttpTimeoutSeconds": 60,
            "ListingRequestDelayMs": 0,
            "StopAfterKnownListingPages": 5,
            "MaxListingPages": 1000,
            "RecentIdWindowSize": 500
          },
          "Import": {
            "MaxDegreeOfParallelism": 2,
            "RangeModeDefaultCount": 1500,
            "EmptyDatasetRangeCount": 90000
          }
        }
        """;

    [Fact]
    public void Parse_ValidJson_ReturnsAllValues()
    {
        var settings = AppSettings.Parse(ValidJson);

        Assert.Equal("odbor", settings.Uohs.AreaFilterFormField);
        Assert.Equal(TimeSpan.FromSeconds(60), settings.Uohs.HttpTimeout);
        Assert.Equal(TimeSpan.Zero, settings.Uohs.ListingRequestDelay);
        Assert.Equal(5, settings.Uohs.StopAfterKnownListingPages);
        Assert.Equal(1000, settings.Uohs.MaxListingPages);
        Assert.Equal(500, settings.Uohs.RecentIdWindowSize);
        Assert.Equal(2, settings.Import.MaxDegreeOfParallelism);
        Assert.Equal(1500, settings.Import.RangeModeDefaultCount);
        Assert.Equal(90000, settings.Import.EmptyDatasetRangeCount);
    }

    [Fact]
    public void Parse_MissingValues_ThrowsListingEveryMissingValue()
    {
        var json = ValidJson
            .Replace("\"AreaFilterFormField\": \"odbor\",", "")
            .Replace("\"MaxDegreeOfParallelism\": 2,", "");

        var error = Assert.Throws<InvalidOperationException>(() => AppSettings.Parse(json));

        Assert.Contains("Uohs:AreaFilterFormField", error.Message);
        Assert.Contains("Import:MaxDegreeOfParallelism", error.Message);
    }

    [Fact]
    public void Parse_MissingSection_Throws()
    {
        var error = Assert.Throws<InvalidOperationException>(() => AppSettings.Parse("""{ "Uohs": null }"""));

        Assert.Contains("'Uohs'", error.Message);
        Assert.Contains("'Import'", error.Message);
    }

    [Fact]
    public void Parse_TemplateWithoutPlaceholder_Throws()
    {
        var json = ValidJson.Replace("detail-{0}.html", "detail.html");

        var error = Assert.Throws<InvalidOperationException>(() => AppSettings.Parse(json));

        Assert.Contains("Uohs:DetailUrlTemplate", error.Message);
    }

    [Fact]
    public void Parse_NumberBelowMinimum_Throws()
    {
        var json = ValidJson.Replace("\"StopAfterKnownListingPages\": 5", "\"StopAfterKnownListingPages\": 0");

        var error = Assert.Throws<InvalidOperationException>(() => AppSettings.Parse(json));

        Assert.Contains("Uohs:StopAfterKnownListingPages", error.Message);
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AppSettings.Parse("{ not json"));
    }

    [Fact]
    public void Load_MissingFile_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), AppSettings.FileName);

        Assert.Throws<InvalidOperationException>(() => AppSettings.Load(path));
    }

    [Fact]
    public void Load_ShippedConfigurationFile_IsValid()
    {
        var path = Path.Combine(AppContext.BaseDirectory, AppSettings.FileName);

        var settings = AppSettings.Load(path);

        Assert.NotNull(settings);
    }
}

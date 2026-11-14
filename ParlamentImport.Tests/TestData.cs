namespace HlidacStatu.ParlamentImport.Tests;

/// <summary>Shared options and fixture access; fixtures are real responses saved on 2026-11-14.</summary>
internal static class TestData
{
    public static ImportOptions Options { get; } = new()
    {
        PspOpenDataZipUrl = "https://psp.test/poslanci.zip",
        PspPersonUrlTemplate = "https://psp.test/detail?id={0}",
        SenatSenatorListUrl = "https://senat.test/list",
        SenatLeadershipUrl = "https://senat.test/index",
        SenatClubListUrl = "https://senat.test/kluby",
        SenatClubUrlTemplate = "https://senat.test/klub?id={0}",
        SenatPersonUrlTemplate = "https://senat.test/senator?id={0}",
        EpApiBaseUrl = "https://ep.test/api/v2/",
        EpCountryCode = "CZ",
        EpPageSize = 100,
        EpPersonUrlTemplate = "https://ep.test/meps/{0}",
        UserAgent = "test",
        HttpTimeoutSeconds = 1,
        RequestDelayMilliseconds = 1,
        ClubLabelMaxLength = 16,
        ClubNamePrefixes = ["Poslanecký klub ", "Senátorský klub "],
        UnaffiliatedClubNamePrefixes = ["Nezařazení", "Senátoři nezařazení"],
        UnaffiliatedClubLabel = "nezařazení",
        HlidacApiBaseUrl = "https://api.test",
        PersonSearchStatus = 1,
        PersonApiDelayMilliseconds = 1,
        PspRolePrefixes = ["poslanec", "poslankyně"],
        PspOrganizationPrefixes = ["poslanecká sněmovna"],
        SenatRolePrefixes = ["senát"],
        SenatOrganizationPrefixes = ["senát"],
        EpRolePrefixes = ["poslanec ep", "poslankyně ep", "europoslan"],
        EpOrganizationPrefixes = ["evropský parlament"],
        MinimumPspMembers = 1,
        MinimumSenatMembers = 1,
        MinimumEpMembers = 1,
    };

    public static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
}

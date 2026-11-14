using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HlidacStatu.ParlamentImport;

/// <summary>Chamber a <see cref="ParliamentMember"/> belongs to; stored in the dataset as its name.</summary>
public enum ParliamentChamber
{
    /// <summary>Poslanecká sněmovna Parlamentu ČR.</summary>
    Psp,

    /// <summary>Senát Parlamentu ČR.</summary>
    Senat,

    /// <summary>Evropský parlament, members elected in the Czech Republic.</summary>
    Ep,
}

/// <summary>
/// One record of the public dataset "parlament-slozeni". The JSON property names are the contract of the dataset:
/// the Web reads the same records as HlidacStatu.Entities.Parlament.ParliamentMemberRecord, so a rename here
/// must be made there too (and in the registered JSON schema).
/// </summary>
public sealed record ParliamentMember
{
    public const string DatasetId = "parlament-slozeni";

    /// <summary>Stable id "{chamber}-{source person id}", e.g. "psp-6105", "senat-243", "ep-124701".</summary>
    [JsonProperty("id")]
    public required string Id { get; init; }

    [JsonProperty("chamber")]
    [JsonConverter(typeof(StringEnumConverter))]
    public required ParliamentChamber Chamber { get; init; }

    [JsonProperty("firstName")]
    public required string FirstName { get; init; }

    [JsonProperty("lastName")]
    public required string LastName { get; init; }

    [JsonProperty("titleBefore")]
    public string? TitleBefore { get; init; }

    [JsonProperty("titleAfter")]
    public string? TitleAfter { get; init; }

    /// <summary>Full name with titles, as shown on the source web.</summary>
    [JsonProperty("fullName")]
    public required string FullName { get; init; }

    /// <summary>Date of birth when the source publishes it (psp.cz, European Parliament).</summary>
    [JsonProperty("birthDate")]
    public DateTime? BirthDate { get; init; }

    /// <summary>NameId of the matched Hlídač státu person; null when the match was not unique.</summary>
    [JsonProperty("nameId")]
    public string? NameId { get; init; }

    /// <summary>Full name of the parliamentary club or EP political group.</summary>
    [JsonProperty("club")]
    public string? Club { get; init; }

    /// <summary>Short label of the club or EP group used in charts and filters.</summary>
    [JsonProperty("clubLabel")]
    public string? ClubLabel { get; init; }

    /// <summary>Id of the club in the source system (psp.cz organ id, senat.cz club id, EP body id).</summary>
    [JsonProperty("clubSourceId")]
    public string? ClubSourceId { get; init; }

    /// <summary>Party the member was elected for or belongs to, as published by the source.</summary>
    [JsonProperty("party")]
    public string? Party { get; init; }

    /// <summary>Senate electoral district number.</summary>
    [JsonProperty("districtNumber")]
    public int? DistrictNumber { get; init; }

    /// <summary>Senate electoral district name.</summary>
    [JsonProperty("districtName")]
    public string? DistrictName { get; init; }

    /// <summary>Leadership function in the chamber, e.g. "předseda", "místopředseda", "1. místopředseda".</summary>
    [JsonProperty("function")]
    public string? Function { get; init; }

    /// <summary>Order of the leadership function, 1 = chair; null for members without one.</summary>
    [JsonProperty("functionOrder")]
    public int? FunctionOrder { get; init; }

    /// <summary>Profile of the member on the source web.</summary>
    [JsonProperty("sourceUrl")]
    public required string SourceUrl { get; init; }

    /// <summary>UTC time of the import run that wrote the record.</summary>
    [JsonProperty("importedUtc")]
    public required DateTime ImportedUtc { get; init; }
}

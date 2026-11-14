using System.Globalization;
using System.IO.Compression;

namespace HlidacStatu.ParlamentImport.Psp;

/// <summary>
/// Builds the current members of the Chamber of Deputies from the psp.cz open data archive poslanci.zip
/// (documented at https://www.psp.cz/sqw/hp.sqw?k=1301). The current term is the organ of type
/// "Poslanecká sněmovna" without an end date; current placements (zarazeni) have no end date.
/// </summary>
internal sealed class PspOpenDataParser(ImportOptions options, ClubLabeler clubLabeler)
{
    /// <summary>typ_organu.id_typ_org of an electoral term of the Chamber of Deputies.</summary>
    private const int OrganTypeChamberTerm = 11;

    /// <summary>typ_organu.id_typ_org of a deputies' club.</summary>
    private const int OrganTypeClub = 1;

    /// <summary>typ_funkce.typ_funkce_obecny of a chair.</summary>
    private const int GeneralFunctionChair = 1;

    /// <summary>typ_funkce.typ_funkce_obecny of a vice-chair.</summary>
    private const int GeneralFunctionViceChair = 2;

    private const string PlacementInOrgan = "0";
    private const string PlacementInFunction = "1";

    private sealed record Organ(int Id, int? ParentId, int TypeId, string? Abbreviation, string Name, DateTime? From, DateTime? To);

    private sealed record Person(int Id, string? TitleBefore, string LastName, string FirstName, string? TitleAfter, DateTime? BirthDate);

    private sealed record Placement(int PersonId, int OfId, string Kind, DateTime? To);

    private sealed record Function(int Id, int OrganId, int TypeId, string Name);

    /// <summary>Reads the UNL tables from the archive and returns the current deputies.</summary>
    public IReadOnlyList<ParliamentMember> Parse(Stream zipArchive, DateTime importedUtc)
    {
        ArgumentNullException.ThrowIfNull(zipArchive);
        using var archive = new ZipArchive(zipArchive, ZipArchiveMode.Read);

        IReadOnlyList<string?[]> Table(string name)
        {
            var entry = archive.GetEntry(name + ".unl")
                        ?? throw new InvalidDataException($"poslanci.zip does not contain {name}.unl");
            using var reader = new StreamReader(entry.Open(), UnlReader.SourceEncoding);
            return UnlReader.Parse(reader.ReadToEnd());
        }

        return Parse(Table("organy"), Table("osoby"), Table("zarazeni"), Table("funkce"), Table("typ_funkce"),
            Table("poslanec"), importedUtc);
    }

    /// <summary>Builds the current deputies from already parsed UNL tables.</summary>
    public IReadOnlyList<ParliamentMember> Parse(
        IReadOnlyList<string?[]> organRows, IReadOnlyList<string?[]> personRows, IReadOnlyList<string?[]> placementRows,
        IReadOnlyList<string?[]> functionRows, IReadOnlyList<string?[]> functionTypeRows, IReadOnlyList<string?[]> deputyRows,
        DateTime importedUtc)
    {
        var organs = organRows.Select(r => new Organ(
                UnlReader.ParseInt(r[0], "organy.id_organ"),
                r[1] is null ? null : UnlReader.ParseInt(r[1], "organy.organ_id_organ"),
                UnlReader.ParseInt(r[2], "organy.id_typ_organu"),
                r[3], r[4] ?? string.Empty, UnlReader.ParseDate(r[6]), UnlReader.ParseDate(r[7])))
            .ToDictionary(o => o.Id);

        var term = organs.Values
                       .Where(o => o.TypeId == OrganTypeChamberTerm && o.To is null)
                       .MaxBy(o => o.From)
                   ?? throw new InvalidDataException("poslanci.zip has no open electoral term of the Chamber of Deputies.");

        var clubs = organs.Values.Where(o => o.TypeId == OrganTypeClub && o.ParentId == term.Id).ToDictionary(o => o.Id);

        var persons = personRows.Select(r => new Person(
                UnlReader.ParseInt(r[0], "osoby.id_osoba"), r[1], r[2] ?? string.Empty, r[3] ?? string.Empty, r[4],
                UnlReader.ParseDate(r[5])))
            .ToDictionary(p => p.Id);

        var placements = placementRows
            .Select(r => new Placement(
                UnlReader.ParseInt(r[0], "zarazeni.id_osoba"), UnlReader.ParseInt(r[1], "zarazeni.id_of"),
                r[2] ?? string.Empty, UnlReader.ParseDateTime(r[4])))
            .Where(p => p.To is null)
            .ToList();

        var generalFunctionByType = functionTypeRows
            .Where(r => r[5] is not null)
            .ToDictionary(r => UnlReader.ParseInt(r[0], "typ_funkce.id_typ_funkce"),
                r => UnlReader.ParseInt(r[5], "typ_funkce.typ_funkce_obecny"));

        var leadershipFunctions = functionRows
            .Select(r => new Function(UnlReader.ParseInt(r[0], "funkce.id_funkce"), UnlReader.ParseInt(r[1], "funkce.id_organ"),
                UnlReader.ParseInt(r[2], "funkce.id_typ_funkce"), r[3] ?? string.Empty))
            .Where(f => f.OrganId == term.Id
                        && generalFunctionByType.TryGetValue(f.TypeId, out var general)
                        && general is GeneralFunctionChair or GeneralFunctionViceChair)
            .ToDictionary(f => f.Id);

        var candidateListByPerson = deputyRows
            .Where(r => r[4] is not null && UnlReader.ParseInt(r[4], "poslanec.id_obdobi") == term.Id && r[3] is not null)
            .GroupBy(r => UnlReader.ParseInt(r[1], "poslanec.id_osoba"))
            .ToDictionary(g => g.Key, g => UnlReader.ParseInt(g.Last()[3], "poslanec.id_kandidatka"));

        var memberIds = placements
            .Where(p => p.Kind == PlacementInOrgan && p.OfId == term.Id)
            .Select(p => p.PersonId)
            .Distinct()
            .ToList();

        List<ParliamentMember> members = [];
        foreach (var personId in memberIds)
        {
            var person = persons.TryGetValue(personId, out var p)
                ? p
                : throw new InvalidDataException($"poslanci.zip: deputy {personId} is missing in osoby.unl.");

            var club = placements
                .Where(x => x.PersonId == personId && x.Kind == PlacementInOrgan && clubs.ContainsKey(x.OfId))
                .Select(x => clubs[x.OfId])
                .FirstOrDefault();

            var function = placements
                .Where(x => x.PersonId == personId && x.Kind == PlacementInFunction && leadershipFunctions.ContainsKey(x.OfId))
                .Select(x => leadershipFunctions[x.OfId])
                .OrderBy(f => generalFunctionByType[f.TypeId])
                .FirstOrDefault();

            string? party = candidateListByPerson.TryGetValue(personId, out var listId) && organs.TryGetValue(listId, out var list)
                ? list.Name
                : null;

            members.Add(new ParliamentMember
            {
                Id = "psp-" + personId.ToString(CultureInfo.InvariantCulture),
                Chamber = ParliamentChamber.Psp,
                FirstName = person.FirstName,
                LastName = person.LastName,
                TitleBefore = person.TitleBefore,
                TitleAfter = person.TitleAfter,
                FullName = string.Join(' ', new[] { person.TitleBefore, person.FirstName, person.LastName }
                               .Where(s => !string.IsNullOrWhiteSpace(s)))
                           + (string.IsNullOrWhiteSpace(person.TitleAfter) ? string.Empty : ", " + person.TitleAfter),
                BirthDate = person.BirthDate,
                Club = club?.Name,
                ClubLabel = club is null ? null : clubLabeler.Label(club.Name, club.Abbreviation),
                ClubSourceId = club?.Id.ToString(CultureInfo.InvariantCulture),
                Party = party,
                Function = function?.Name.ToLower(CultureInfo.GetCultureInfo("cs-CZ")),
                FunctionOrder = function is null ? null : generalFunctionByType[function.TypeId],
                SourceUrl = string.Format(CultureInfo.InvariantCulture, options.PspPersonUrlTemplate, personId),
                ImportedUtc = importedUtc,
            });
        }

        return members;
    }
}

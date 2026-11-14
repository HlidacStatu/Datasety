# ParlamentImport

Console application that loads the current composition of the Czech Chamber of Deputies, the Senate and the Czech
members of the European Parliament and stores it in the public dataset **`parlament-slozeni`**
(https://www.hlidacstatu.cz/data/Index/parlament-slozeni). The dataset feeds the `/osoby` "Politici" page of the Web
project (clubs, seat charts, chamber leadership, Senate districts, EP groups).

The application is standalone: it talks to Hlídač státu **only through the public REST API**
(https://api.hlidacstatu.cz) using the NuGet clients `HlidacStatu.Api.V2.Dataset` (dataset) and `HlidacStatu.Api.V2`
(persons). It needs no database, no Elasticsearch and no internal Hlídač státu library.

Projects: `ParlamentImport/` (application, solution `ParlamentImport.sln`) and `ParlamentImport.Tests/` (xunit).
`HlidacStatu.Api.V2.Dataset` 2.4.1.1 is published on the Devmasters NuGet feed, which `nuget.config` adds next to
nuget.org.

The dataset is a **full snapshot**: every run writes all current members and deletes records of members who are no
longer present in the sources.

## Sources

| Chamber | Source | What is read |
|---|---|---|
| Chamber of Deputies (`Psp`) | psp.cz open data archive `poslanci.zip` (UNL tables, windows-1250) | current term, deputies, clubs, chair and vice-chairs, candidate list, date of birth |
| Senate (`Senat`) | senat.cz HTML pages (no machine-readable export exists) | senator list with district and political affiliation, chair and vice-chairs, senatorial clubs and their members |
| European Parliament (`Ep`) | EP Open Data API v2 (`/meps/show-current`, `/meps/{id}`, `/corporate-bodies/{id}`) | current Czech MEPs, date of birth, EP political group, national party |

All URLs are configured in `appsettings.json` (section `ParlamentImport`). Requests to the sources run one after
another with a configured pause between them.

## Running

Build:

```
dotnet build ParlamentImport/ParlamentImport.sln
```

Run from the output directory (`appsettings.json` and `logger.serilog.json` are copied next to the exe):

```
ParlamentImport.exe --apikey <token>                          # full import into the dataset
ParlamentImport.exe --apikey <token> --dry-run result.json    # everything except writing; snapshot saved to result.json
ParlamentImport.exe --apikey <token> --recreate-dataset       # delete the dataset, register it again, import
```

| Argument | Meaning |
|---|---|
| `--apikey <token>` | **Required.** Hlídač státu API token (https://www.hlidacstatu.cz/api). The dataset is registered and written under the account of this token. |
| `--dry-run <path>` | Downloads, parses and matches; the snapshot is saved as JSON (the dataset record format) to `<path>`. The dataset is only read (to reuse NameIds), never written. |
| `--recreate-dataset` | Deletes the dataset and registers it again before writing (e.g. after a change of the record schema). Only the owner of the dataset may delete it. Cannot be combined with `--dry-run`. |
| `--Section:Key=value` | Any configuration value can be overridden, e.g. `--ParlamentImport:RequestDelayMilliseconds=1000`. |

Exit codes: `0` success, `1` the import failed or was cancelled (Ctrl+C), `2` invalid command line.

A daily run is enough; the sources change only when members, clubs or leadership change. The application is not
scheduled anywhere yet.

### API token and dataset owner

- The first run registers the dataset; the account of the token becomes its **owner**. Later runs must use a token
  of the same account: the API lets only the owner write records and delete the dataset (deleting single records is
  also allowed to administrators).
- An invalid token is redirected by the API to the login page; the importer stops with
  "Hlídač státu API ... check the API token (--apikey)".
- Deleting single records needs the endpoint `DELETE /api/v2/datasety/{datasetId}/zaznamy/{itemId}`
  (HlidacStatuApi, `ApiV2DatasetyController.DatasetItem_Delete`). It is not part of the NuGet client, so the importer
  calls it over plain HTTP with the same token. Until that endpoint is deployed, a run that has a member to delete
  fails after writing the current members.

## Configuration (`appsettings.json`, section `ParlamentImport`)

All keys are required; a missing or invalid value stops the run at startup with a message listing the keys.

| Key | Meaning |
|---|---|
| `PspOpenDataZipUrl` | psp.cz `poslanci.zip` |
| `PspPersonUrlTemplate` | deputy profile, `{0}` = psp.cz `id_osoba` |
| `SenatSenatorListUrl`, `SenatLeadershipUrl`, `SenatClubListUrl` | senat.cz pages: senator list, leadership, list of clubs |
| `SenatClubUrlTemplate`, `SenatPersonUrlTemplate` | club page (`{0}` = club id `par_2`), senator profile (`{0}` = senator id `par_3`) |
| `EpApiBaseUrl` | EP Open Data API v2 base, must end with `/` |
| `EpCountryCode`, `EpPageSize` | country of the imported MEPs, page size of the MEP list |
| `EpPersonUrlTemplate` | MEP profile, `{0}` = MEP identifier |
| `UserAgent`, `HttpTimeoutSeconds`, `RequestDelayMilliseconds` | HTTP settings for the sources and the API (EP asks for a User-Agent without personal data) |
| `ClubLabelMaxLength`, `ClubNamePrefixes` | short club label: prefix ("Poslanecký klub ") stripped, a longer label falls back to the source abbreviation |
| `UnaffiliatedClubNamePrefixes`, `UnaffiliatedClubLabel` | how clubs of members without a club are recognised and labelled; the Web reads the same label (`PoliticiPageConfig.UnaffiliatedClubLabel`) |
| `HlidacApiBaseUrl` | Hlídač státu REST API, without a trailing slash |
| `PersonSearchStatus` | `status` of `api/v2/osoby/hledatFtx` (1 = politicians, sponsors, officials) |
| `PersonApiDelayMilliseconds` | pause between two person API calls |
| `PspRolePrefixes`, `PspOrganizationPrefixes`, `SenatRolePrefixes`, `SenatOrganizationPrefixes`, `EpRolePrefixes`, `EpOrganizationPrefixes` | how a current mandate is recognised in the person's functions, see Matching |
| `MinimumPspMembers`, `MinimumSenatMembers`, `MinimumEpMembers` | safety limits, see below |

Logging is configured in `logger.serilog.json`.

## Safety

- If a source yields fewer members than its `Minimum*Members` limit (e.g. a changed HTML layout), the run stops
  **before writing anything**, so a broken parser can never empty the dataset.
- Every Senate club page declares its member count; the parsed members must match it, otherwise the run stops.
- Parsers fail on unexpected data (missing tables, unknown date formats, missing chair of the Senate) instead of
  writing partial records.

## Matching members to Hlídač státu persons

Each record gets `nameId` of the matched person, or `null`:

1. A member already stored in the dataset with a `nameId` keeps it; no API call is made. (After `--recreate-dataset`
   everybody is matched again.)
2. Otherwise persons with the same name are searched (`api/v2/osoby/hledatFtx`, names compared without diacritics,
   case and hyphens). For each of them the detail (`api/v2/osoby/{nameId}`) is read and only people whose functions
   contain a mandate valid today are kept: role starting with a `*RolePrefixes` value and organization starting with
   a `*OrganizationPrefixes` value of the member's chamber (e.g. "Poslanec" at "Poslanecká sněmovna PČR",
   "Europoslanec"/"Poslanec EP" at "Evropský parlament"). One such person is accepted; several are narrowed by the
   date of birth.
3. A member not found that way is matched by first name + surname + exact date of birth (`api/v2/osoby/hledat`) when
   exactly one such person exists. Only psp.cz and the EP publish dates of birth.
4. Anything else stays unmatched; the Web then links the name to the search.

Every decision is logged ("matched ... by role+name", "by previous-run", "not matched ...: ambiguous").

## How the dataset is written

All dataset operations go through the REST API:

| Step | API call (NuGet `HlidacStatu.Api.V2.Dataset`, `DatasetyApi`) |
|---|---|
| Does the dataset exist? | `ApiV2DatasetyDetailAsync` (`GET api/v2/datasety/{id}`) |
| Register it (first run, `--recreate-dataset`) | `ApiV2DatasetyCreateAsync` (`POST api/v2/datasety`) with name, description, JSON schema generated from `ParliamentMember`, search and detail Scriban templates. Every text field of the registration and of the templates is filled (source code URL, default order, sort list, template header/footer as HTML comments): the server treats them all as required and answers a null field with HTTP 500. |
| Delete it (`--recreate-dataset`) | `ApiV2DatasetyDeleteAsync` (`DELETE api/v2/datasety/{id}`) |
| Read stored records | `ApiV2DatasetyDatasetSearchAsync` (`GET api/v2/datasety/{id}/hledat?dotaz=*`), 25 records per page |
| Write a member | `ApiV2DatasetyDatasetItemUpdateAsync` (`POST api/v2/datasety/{id}/zaznamy/{itemId}?mode=rewrite`) |
| Delete a member who left | plain HTTP `DELETE api/v2/datasety/{id}/zaznamy/{itemId}` (not in the NuGet client) |

Records are stored with camelCase property names (`id`, `chamber`, `fullName`, `nameId`, `club`, `clubLabel`, ...).
The importer type is `ParliamentMember`; the Web reads the same JSON as `HlidacStatu.Entities.Parlament.ParliamentMemberRecord`,
so a property change must be made in both. Record ids are stable: `psp-{id_osoba}`, `senat-{senator id}`,
`ep-{MEP identifier}`. The registered JSON schema cannot be changed through the API; after changing the record
type run once with `--recreate-dataset`.

## Logging

Serilog configured by `logger.serilog.json`: structured JSON lines in the daily file
`c:/data/Logs/ParlamentImport/parlamentimport<date>.json` (Serilog `JsonFormatter`) plus the console. Further sinks
(e.g. a central log server) can be added on the target machine in its copy of `logger.serilog.json`. Logged: every source request (URL, status, size, duration), parsed counts, every match
decision, written and deleted records, total duration and failures.

## Tests

`ParlamentImport.Tests` (xunit): parsers on saved real responses (`Fixtures/`, captured 2026-11-14), the psp.cz
parser on small UNL tables, matching rules (with a fake person directory), mandate recognition, command line,
dataset record JSON and registration, club labels and configuration validation. The server side of the record
delete endpoint lives in the Hlídač státu API project (tests `DatasetItemPermissionTests`).

```
dotnet test ParlamentImport/ParlamentImport.sln
```

using System.Collections.Generic;
using System.Linq;

namespace Rozhodnuti_UOHS.Tests;

/// <summary>
/// Builds synthetic HTML shaped like the UOHS decision collection pages (search form, listing table, decision detail).
/// </summary>
internal static class ListingHtml
{
    public const string AreaField = "odbor";

    /// <summary>
    /// Listing page; each entry is (area path segment, decision ID). <paramref name="selectedArea"/> null = no area selected.
    /// </summary>
    public static string Listing(IEnumerable<(string Area, int Id)> decisions, string? selectedArea = null)
    {
        var rows = string.Concat(decisions.Select(d => $"""
            <tr><td><a href="/cs/{d.Area}/sbirky-rozhodnuti/detail-{d.Id}.html">S{d.Id}/2026</a>
            <br/><a href="/cs/{d.Area}/sbirky-rozhodnuti/detail-{d.Id}.html">detail</a></td><td>7.&nbsp;11.&nbsp;2026</td></tr>
            """));

        return Page($"""
            <h1>Sbírky rozhodnutí</h1>
            <table id="resolutions_list">
            <tr><th>číslo / věc / účastníci</th><th class="datum_moci">nabytí pr. moci</th></tr>
            {rows}
            </table>
            <div id="paging"><span>1</span><a class="num" href="/cs/verejne-zakazky/sbirky-rozhodnuti/2.html">2</a></div>
            """, selectedArea);
    }

    public static string Listing(params int[] ids) => Listing(ids.Select(id => ("verejne-zakazky", id)));

    /// <summary>
    /// Decision detail page, which the server returns instead of a listing for some page numbers.
    /// </summary>
    public static string Detail(int id) => Page($"""
        <h1>číslo jednací: <strong>VZ/S{id}/02</strong></h1>
        <table id="resolution_detail"><tr><th>Instance</th><td>I.</td></tr></table>
        <a href="/cs/verejne-zakazky/sbirky-rozhodnuti/detail-{id + 1}.html">související</a>
        """, selectedArea: null);

    static string Page(string content, string? selectedArea)
    {
        string Option(string value, string label) =>
            $"""<option value="{value}" {(value == selectedArea ? "selected=\"selected\"" : "")}>{label}</option>""";

        return $"""
            <!DOCTYPE html>
            <html><head><title>Úřad pro ochranu hospodářské soutěže | Sbírky rozhodnutí</title></head>
            <body>
            <div id="content"><div id="text_l">
            {content}
            <form id="r_search_form" action="/cs/verejne-zakazky/sbirky-rozhodnuti.html" method="post">
            <select name="rok"><option value="">--</option><option value="2026" selected="selected">2026</option></select>
            <select name="{AreaField}">{Option("", "--")}{Option("1", "Hospodářská soutěž")}{Option("2", "Veřejné zakázky")}</select>
            </form>
            </div></div>
            <a href="/cs/verejne-zakazky/sbirky-rozhodnuti/detail-99999.html">odkaz mimo výpis</a>
            </body></html>
            """;
    }
}

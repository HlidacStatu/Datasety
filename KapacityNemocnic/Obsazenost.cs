using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using OfficeOpenXml;

namespace KapacityNemocnic
{
    public class Obsazenost
    {
        static DateTime startDt = DateTime.Now.Date.AddDays(-20); //new DateTime(2020,09,04);
        public static void ProcessExcelObsazenost(string fn, HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData> ds)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage(new System.IO.FileInfo(fn)))
            {
                foreach (var ws in p.Workbook.Worksheets)
                {
                    //first date  2020-09-04

                    for (int row = 9; row < 100000; row++)
                    {
                        var dt = ws.Cells[row, 1].GetValue<DateTime?>();
                        if (dt.HasValue && dt.Value >= startDt)
                        {
                            string id = "id_" + dt.Value.ToString("yyyy-MM-dd");
                            Console.Write(dt.Value.ToString("yyyy-MM-dd "));
                            NemocniceData data = null;
                            try
                            {
                                data =  ds.GetItem(id);
                            }
                            catch (Exception)
                            {
                            }
                            if (data == null)
                                continue;

                            var region = data.regions.FirstOrDefault(m => m.region == NemocniceData.ExcelWorkBookToRegion(ws.Name));
                            if (region != null)
                            {
                                var idx = data.regions.IndexOf(region);
                                data.regions[idx].Pacienti_bezpriznaku = ws.Cells[row, 7].GetValue<int>();
                                data.regions[idx].Pacienti_lehky = ws.Cells[row, 8].GetValue<int>();
                                data.regions[idx].Pacienti_stredni = ws.Cells[row, 9].GetValue<int>();
                                data.regions[idx].Pacienti_tezky = ws.Cells[row, 10].GetValue<int>();
                                data.regions[idx].Pacienti_zemreli = ws.Cells[row, 21].GetValue<int>() - ws.Cells[row - 1, 21].GetValue<int>();
                                Console.Write(" " + region.region);
                                ds.AddOrUpdateItem(data, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                            }
                            else
                                Console.WriteLine("not found region " + ws.Name);

                        }
                    }

                }
            }
        }




    }
}

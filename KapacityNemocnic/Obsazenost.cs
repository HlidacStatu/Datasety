using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using OfficeOpenXml;

using S22.Imap;

namespace KapacityNemocnic
{
    public class Obsazenost
    {
        static string uzisRoot = Program.GetExecutingDirectoryName() + "\\UZIS_Reports\\";

        public static void LastObsazenost(HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData> ds)
        {
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now.Date.AddDays(-1 * i);
                string reportDir = uzisRoot + dt.ToString("yyyy-MM-dd") + "\\";
                if (System.IO.Directory.Exists(reportDir))
                {
                    foreach (var fn in System.IO.Directory.EnumerateFiles(reportDir, "*.xlsx"))
                    {
                        if (fn.Contains("hosp04_KRAJE"))
                        {
                            Devmasters.Logging.Logger.Root.Info($"Process lastObsazenost {fn}");
                            Obsazenost.ProcessExcelObsazenost(fn, ds);
                            return;
                        }

                    }

                }
            }
        }

        public static void Imap(string password, HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData> ds)
        {

            string obsazenostFile = null;

            Devmasters.Logging.Logger.Root.Info("connection to uzisbackupmbx@");

            using (ImapClient Client = new ImapClient("imap.gmail.com", 993,
                 "uzisbackupmbx@gmail.com", password, AuthMethod.Login, true))
            {
                // This returns all messages sent since August 23rd 2012.
                IEnumerable<uint> uids = Client.Search(
                    SearchCondition.Unseen()
                    
                 );

                Devmasters.Logging.Logger.Root.Info($"found {uids.Count()} email");

                // The expression will be evaluated for every MIME part
                // of every mail message in the uids collection.
                IEnumerable<MailMessage> messages = Client.GetMessages(uids, true);
                foreach (var msg in messages)
                {
                    string reportDir =uzisRoot + msg.Date()?.ToString("yyyy-MM-dd")+"\\";
                    if (System.IO.Directory.Exists(reportDir) == false)
                        System.IO.Directory.CreateDirectory(reportDir);


                    Devmasters.Logging.Logger.Root.Info($"saving {msg.Attachments?.Count()} files");

                    foreach (var att in msg.Attachments)
                    {
                        if (att.ContentDisposition.Inline== false)
                        {
                            var attfn = MakeValidFileName(att.Name);
                            using (var fs = System.IO.File.Create(reportDir+attfn))
                            {
                                att.ContentStream.Seek(0, System.IO.SeekOrigin.Begin);
                                att.ContentStream.CopyTo(fs);
                            }
                            if (attfn.Contains("hosp04_KRAJE"))
                            {
                                obsazenostFile = reportDir + attfn;
                                Obsazenost.ProcessExcelObsazenost(obsazenostFile, ds);
                            }
                        }
                    }

                }

            }

        }
        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "");
        }
        
        public static void BackupImap()
        {
            string uzisRoot = Program.GetExecutingDirectoryName() + "\\UZIS_Reports\\";

            string obsazenostFile = null;

            using (ImapClient Client = new ImapClient("imap.gmail.com", 993,
                 "", "", AuthMethod.Login, true))
            {
                // This returns all messages sent since August 23rd 2012.
                IEnumerable<uint> uids = Client.Search(
                    SearchCondition.From("Ladislav.Dusek@uzis.cz")
                    .And(SearchCondition.To("vz@psp.cz"))
                    //SearchCondition.All()
                 );


                // The expression will be evaluated for every MIME part
                // of every mail message in the uids collection.
                foreach (var uid in uids)
                {
                    Console.WriteLine(uid );
                    var msg = Client.GetMessage(uid);
                    Console.Write($" {msg.Date()} ");

                    string reportDir = uzisRoot + msg.Date()?.ToString("yyyy-MM-dd") + "\\";
                    if (System.IO.Directory.Exists(reportDir) == false)
                        System.IO.Directory.CreateDirectory(reportDir);

                    foreach (var att in msg.Attachments)
                    {
                        Console.Write(".");
                        if (att.ContentDisposition.Inline == false)
                        {
                            using (var fs = System.IO.File.Create(reportDir + MakeValidFileName(att.Name)))
                            {
                                att.ContentStream.Seek(0, System.IO.SeekOrigin.Begin);
                                att.ContentStream.CopyTo(fs);
                            }
                        }
                    }
                    Console.WriteLine();

                }

            }

        }


        static DateTime startDt = DateTime.Now.Date.AddDays(-10); //new DateTime(2020,09,04);
        public static void ProcessExcelObsazenost(string fn, HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData> ds)
        {
            Devmasters.Logging.Logger.Root.Info($"ProcessExcelObsazenost {fn} ");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage(new System.IO.FileInfo(fn)))
            {
                foreach (var ws in p.Workbook.Worksheets)
                {
                    //first date  2020-09-04

                    for (int row = 11; row < 100000; row++)
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
                                data.regions[idx].Pacienti_zemreli = ws.Cells[row, 22].GetValue<int>() - ws.Cells[row - 1, 22].GetValue<int>();

                                Devmasters.Logging.Logger.Root.Info($"ProcessExcelObsazenost save {ws.Name} - {dt.Value.ToString("yyyy-MM-dd ")} - {region.region} ");
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

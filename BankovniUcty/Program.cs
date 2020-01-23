using HlidacStatu.Api.Dataset.Connector;
using HlidacStatu.Lib.ES;
using HlidacStatu.Lib.Data.TransparentniUcty;
using Devmasters.Core;
using System.Collections.Generic;
using System;
using Nest;
using System.Linq;

namespace BankovniUcty
{
    class Program
    {
        static DatasetConnector dsc;
        static void Main(string[] args)
        {
            dsc = new DatasetConnector( 
                System.Configuration.ConfigurationManager.AppSettings["apikey"]
                );

            //create dataset účtu
            var dsUcet = new Dataset<BankovniUcet>(
                "Transparentní účty", 
                "Transparentni-ucty", 
                "http://www.hlidacstatu.cz", 
                "Seznam transparentních účtů.",
                "https://github.com/HlidacStatu/Datasety/tree/master/BankovniUcty",
                false, 
                true,
                new string[,] { { "Číslo účtu", "CisloUctu" }, { "Naposledy parsováno", "LastSuccessfullParsing" } },
                new ClassicTemplate.ClassicSearchResultTemplate()
                    .AddColumn("Číslo účtu", @"<a href=""@(fn_DatasetItemUrl(item.Id))"">@item.CisloUctu</a>")
                    .AddColumn("Název", "@item.Nazev")
                    .AddColumn("Subjekt", "@item.Subjekt")
                    .AddColumn("Naposledy parsováno", "@fn_FormatDate(item.LastSuccessfullParsing,\"dd.MM.yyyy\")")
                ,
                new ClassicTemplate.ClassicDetailTemplate()
                    .AddColumn("Číslo účtu", "@item.CisloUctu")
                    .AddColumn("Název", "@item.Nazev")
                    .AddColumn("Subjekt", "@item.Subjekt")
                    .AddColumn("Typ Subjektu", "@item.TypSubjektu")
                    .AddColumn("Měna", "@item.Mena")
                    .AddColumn("Naposledy parsováno", "@fn_FormatDate(item.LastSuccessfullParsing,\"dd.MM.yyyy\")")
                    .AddColumn("Zdroj", "@item.Url")
                    .AddColumn("Je aktivní", "@item.Active")
                    .AddColumn("Typ Účtu", "@item.TypUctu")
                );

            //create dataset účtu
            var dsPolozka = new Dataset<BankovniPolozka>(
                "Transakce z transparentních účtů",
                "Transakce-ucty",
                "http://www.hlidacstatu.cz",
                "Seznam transakcí z transparentních účtů.",
                "https://github.com/HlidacStatu/Datasety/tree/master/BankovniUcty",
                false,
                true,
                new string[,] { { "Číslo účtu", "CisloUctu" }, { "Naposledy parsováno", "LastSuccessfullParsing" } },
                new ClassicTemplate.ClassicSearchResultTemplate()
                    .AddColumn("ID transakce", @"<a href=""@(fn_DatasetItemUrl(item.Id))"">@item.Id</a>")
                    .AddColumn("Číslo účtu", "@item.CisloUctu")
                    .AddColumn("Subjekt", "@item.Subjekt")
                    .AddColumn("Naposledy parsováno", "@fn_FormatDate(item.LastSuccessfullParsing,\"dd.MM.yyyy\")")
                ,
                new ClassicTemplate.ClassicDetailTemplate()
                    .AddColumn("Číslo účtu", "@item.CisloUctu")
                    .AddColumn("Název", "@item.Nazev")
                    .AddColumn("Subjekt", "@item.Subjekt")
                    .AddColumn("Typ Subjektu", "@item.TypSubjektu")
                    .AddColumn("Měna", "@item.Mena")
                    .AddColumn("Naposledy parsováno", "@fn_FormatDate(item.LastSuccessfullParsing,\"dd.MM.yyyy\")")
                    .AddColumn("Zdroj", "@item.Url")
                    .AddColumn("Je aktivní", "@item.Active")
                    .AddColumn("Typ Účtu", "@item.TypUctu")
                );

            // create dataset in hlidac
            var datasetUcet = dsc.CreateDataset<BankovniUcet>(dsUcet).Result;
            var datasetPolozka = dsc.CreateDataset<BankovniPolozka>(dsPolozka).Result;

            // populate dataset with data
            ImportBankovniUcty(dsUcet);
            ImportBankovniPolozky(dsPolozka);

            Console.WriteLine("done");
            
        }

        static void ImportBankovniUcty(Dataset<BankovniUcet> ds)
        {
            var ucty = HlidacStatu.Lib.Data.TransparentniUcty.BankovniUcty.GetAll();

            foreach (var ucet in ucty)
            {
                var bu = new BankovniUcet()
                {
                    Id = Devmasters.Core.TextUtil.NormalizeToURL(ucet.Id),
                    CisloUctu = ucet.CisloUctu,
                    Mena = ucet.Mena,
                    Nazev = ucet.Nazev,
                    Url = ucet.Url,
                    Subjekt = ucet.Subjekt,
                    TypSubjektu = ucet.TypSubjektu,
                    LastSuccessfullParsing = ucet.LastSuccessfullParsing,
                    TypUctu = ucet.TypUctu.ToNiceDisplayName(),
                    Active = ucet.Active
                };

                dsc.AddItemToDataset<BankovniUcet>(ds, bu);
            }
        }

        static void ImportBankovniPolozky(Dataset<BankovniPolozka> ds)
        {
            var client = Manager.GetESClient_BankovniPolozky();

            var polozky = GetAllDocumentsInIndex<HlidacStatu.Lib.Data.TransparentniUcty.BankovniPolozka>(client);

            foreach (var polozka in polozky)
            {
                var bp = new BankovniPolozka()
                {
                    Id = Devmasters.Core.TextUtil.NormalizeToURL(polozka.Id),
                    Castka = polozka.Castka,
                    CisloProtiuctu = polozka.CisloProtiuctu,
                    CisloUctu = polozka.CisloUctu,
                    Datum = polozka.Datum,
                    KS = polozka.KS,
                    NazevProtiuctu = polozka.NazevProtiuctu,
                    PopisTransakce = polozka.PopisTransakce,
                    SS = polozka.SS,
                    VS = polozka.VS,
                    ZdrojUrl = polozka.ZdrojUrl,
                    Zprava = polozka.ZpravaProPrijemce
                };

                dsc.AddItemToDataset<BankovniPolozka>(ds, bp);
            }
        }

        public static IEnumerable<T> GetAllDocumentsInIndex<T>(ElasticClient elasticClient, string scrollTimeout = "2m", int scrollSize = 1000) where T : class
        {
            ISearchResponse<T> initialResponse = elasticClient.Search<T>
                (scr => scr.From(0)
                     .Take(scrollSize)
                     .MatchAll()
                     .Scroll(scrollTimeout));

            List<T> results = new List<T>();

            if (!initialResponse.IsValid || string.IsNullOrEmpty(initialResponse.ScrollId))
                throw new Exception(initialResponse.ServerError.Error.Reason);

            if (initialResponse.Documents.Any())
                results.AddRange(initialResponse.Documents);

            string scrollid = initialResponse.ScrollId;
            bool isScrollSetHasData = true;
            while (isScrollSetHasData)
            {
                ISearchResponse<T> loopingResponse = elasticClient.Scroll<T>(scrollTimeout, scrollid);
                if (loopingResponse.IsValid)
                {
                    results.AddRange(loopingResponse.Documents);
                    scrollid = loopingResponse.ScrollId;
                }
                isScrollSetHasData = loopingResponse.Documents.Any();
            }

            elasticClient.ClearScroll(new ClearScrollRequest(scrollid));
            return results;
        }

    }
}

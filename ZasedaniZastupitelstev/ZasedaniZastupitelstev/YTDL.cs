using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZasedaniZastupitelstev
{
    public class YTDL
    {

        public class ytdlInfo
        {
            public string uploader_url { get; set; }
            public string _filename { get; set; }
            public Thumbnail[] thumbnails { get; set; }
            public long? abr { get; set; }
            public Subtitles subtitles { get; set; }
            public long? width { get; set; }
            public string[] tags { get; set; }
            public string channel_id { get; set; }
            public string vcodec { get; set; }
            public string acodec { get; set; }
            public string description { get; set; }
            public string uploader { get; set; }
            public object season_number { get; set; }
            public long? age_limit { get; set; }
            public string format { get; set; }
            public object end_time { get; set; }
            public object episode_number { get; set; }
            public object series { get; set; }
            public float? average_rating { get; set; }
            public object start_time { get; set; }
            public object artist { get; set; }
            public string id { get; set; }
            public object license { get; set; }
            public string fulltitle { get; set; }
            public Automatic_Captions automatic_captions { get; set; }
            public string extractor { get; set; }
            public object stretched_ratio { get; set; }
            public object album { get; set; }
            public object release_date { get; set; }
            public object alt_title { get; set; }
            public long? view_count { get; set; }
            public string extractor_key { get; set; }
            public string channel_url { get; set; }
            public object vbr { get; set; }
            public object chapters { get; set; }
            public string ext { get; set; }
            public object playlist_index { get; set; }
            public string[] categories { get; set; }
            public object creator { get; set; }
            public string format_id { get; set; }
            public string uploader_id { get; set; }
            public object annotations { get; set; }
            public string webpage_url { get; set; }
            public Requested_Formats[] requested_formats { get; set; }
            public string title { get; set; }
            public object playlist { get; set; }
            public long? duration { get; set; }
            public object like_count { get; set; }
            public long? fps { get; set; }
            public object requested_subtitles { get; set; }
            public string upload_date { get; set; }
            public object resolution { get; set; }
            public object dislike_count { get; set; }
            public long? height { get; set; }
            public object release_year { get; set; }
            public string thumbnail { get; set; }
            public string display_id { get; set; }
            public object is_live { get; set; }
            public string webpage_url_basename { get; set; }
            public Format[] formats { get; set; }
            public object track { get; set; }
        }

        public class Subtitles
        {
        }

        public class Automatic_Captions
        {
        }

        public class Thumbnail
        {
            public string resolution { get; set; }
            public string id { get; set; }
            public long? height { get; set; }
            public string url { get; set; }
            public long? width { get; set; }
        }

        public class Requested_Formats
        {
            public Downloader_Options downloader_options { get; set; }
            public string filesize { get; set; }
            public long? width { get; set; }
            public long? fps { get; set; }
            public string format { get; set; }
            public string vcodec { get; set; }
            public string acodec { get; set; }
            public string ext { get; set; }
            public long? asr { get; set; }
            public string protocol { get; set; }
            public long? height { get; set; }
            public string url { get; set; }
            public string format_id { get; set; }
            public Http_Headers http_headers { get; set; }
            public float? tbr { get; set; }
            public object player_url { get; set; }
            public string format_note { get; set; }
            public long? abr { get; set; }
        }

        public class Downloader_Options
        {
            public long? http_chunk_size { get; set; }
        }

        public class Http_Headers
        {
            public string UserAgent { get; set; }
            public string Accept { get; set; }
            public string AcceptLanguage { get; set; }
            public string AcceptCharset { get; set; }
            public string AcceptEncoding { get; set; }
        }

        public class Format
        {
            public Downloader_Options1 downloader_options { get; set; }
            public long? filesize { get; set; }
            public string vcodec { get; set; }
            public string format { get; set; }
            public long? width { get; set; }
            public long? fps { get; set; }
            public float? tbr { get; set; }
            public long? abr { get; set; }
            public string acodec { get; set; }
            public string ext { get; set; }
            public long? asr { get; set; }
            public string protocol { get; set; }
            public long? height { get; set; }
            public string url { get; set; }
            public string format_id { get; set; }
            public Http_Headers1 http_headers { get; set; }
            public string format_note { get; set; }
            public object player_url { get; set; }
            public string container { get; set; }
        }

        public class Downloader_Options1
        {
            public long? http_chunk_size { get; set; }
        }

        public class Http_Headers1
        {
            public string UserAgent { get; set; }
            public string Accept { get; set; }
            public string AcceptLanguage { get; set; }
            public string AcceptCharset { get; set; }
            public string AcceptEncoding { get; set; }
        }



        public static Record GetVideoInfo(string youtubeUrl)
        {
            //var fn = System.IO.Path.GetTempFileName();

            System.Diagnostics.ProcessStartInfo piv =
                new System.Diagnostics.ProcessStartInfo("youtube-dl.exe",
                    $"-j {youtubeUrl}"
                    );
            Devmasters.ProcessExecutor pev = new Devmasters.ProcessExecutor(piv, 60 * 6 * 24);
            //pev.StandardOutputDataReceived += (o, e) => { logger.Debug(e.Data); };

            Program.logger.Info("Starting Youtube-dl info for {youtubeUrl} ", youtubeUrl);
            pev.Start();

            ytdlInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<ytdlInfo>(pev.StandardOutput);
            if (info == null)
                return null;
            Record rec = new Record();

            //datum uploadu
            rec.datum = DateTime.ParseExact(info.upload_date, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal);

            var dates = Devmasters.RegexUtil.GetRegexGroupValues(info.title, @"(?<dat>( \d{1,2}\.\s?\d{1,2}\.\s?\d{2,4}) )", "dat");
            if (dates != null && dates.Count() > 0)
            {
                rec.datum = Devmasters.DT.Util.ToDate(dates[0]) ?? rec.datum;
            }


        rec.id = Record.UniqueID(youtubeUrl);
        var queryDictionary = System.Web.HttpUtility.ParseQueryString(new Uri(youtubeUrl).Query);
        rec.ico = "";
            rec.nazev = info.title;
            rec.popis = info.description;
            rec.url = youtubeUrl;
            rec.tags = info.tags;
            rec.delka = info.duration;
            return rec;
        }
}
}

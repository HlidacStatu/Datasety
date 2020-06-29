using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace YoutubeToVyjadreniPolitiku
{
    class YT
    {

        public static record process(string vid)
        {



            Console.WriteLine($"Starting with video {vid}");

            //var vid = "YfvTjQ9MCwY";// "te-t5TDm2BE";
            var url = "https://www.youtube.com/watch?v=" + vid;
            var youtube = new YoutubeClient();
            var videoInfo = youtube.Videos.GetAsync(vid).Result;

            string recId = Devmasters.Core.CryptoLib.Hash.ComputeHashToHex(videoInfo.Url).ToLower();


            if (Program.api.ApiV2DatasetyDatasetItemExists(Program.DataSetId, recId))
                return null;
            
            
            var streamManifest = youtube.Videos.Streams.GetManifestAsync(vid).Result;
            var streamInfo = streamManifest.GetAudioOnly()
                .FirstOrDefault(m => m.Container.Name == "mp4" || m.Container.Name == "mp3")
                ?? streamManifest.GetAudioOnly().WithHighestBitrate();

            var trackManifest = youtube.Videos.ClosedCaptions.GetManifestAsync(vid).Result;
            var trackInfo = trackManifest.TryGetByLanguage("cs");
            if (trackInfo != null)
            {
                // Get the actual closed caption track
                var track = youtube.Videos.ClosedCaptions.GetAsync(trackInfo).Result;
            }

            if (streamInfo != null)
            {

                Console.WriteLine($"Getting MP3 stream for {vid}");

                // Get the actual stream
                var stream = youtube.Videos.Streams.GetAsync(streamInfo).Result;

                string tmpFile = "";
                try
                {
                    var converter = new YoutubeConverter(youtube); // re-using the same client instance for efficiency, not required
                    tmpFile = System.IO.Path.GetTempFileName();
                    var prg = new progress(vid);
                    converter.DownloadAndProcessMediaStreamsAsync(new IStreamInfo[] { streamInfo },
                        tmpFile, "mp3",prg).Wait();

                    Console.WriteLine($"Getting text from MP3 stream for {vid}");

                    var s2t_id = "ntx.v2t.engine.EngineService/cz/t-broadcast/v2t";
                    var s2t_label = "vad+v2t+ppc+pnc";
                    var t2s = new Newton.SpeechToText.Cloud.FileAPI.VoiceToText(tmpFile,
                        Devmasters.Core.Util.Config.GetConfigValue("Newton.Text2Speech.Login"), Devmasters.Core.Util.Config.GetConfigValue("Newton.Text2Speech.Pswd"),
                        s2t_id, s2t_label);
                    t2s.Convert();
                    var _txt = t2s.Text(true);

                    var ret = new record();
                    ret.id = recId;
                    ret.datum = videoInfo.UploadDate.DateTime;
                    ret.origid = videoInfo.Id.Value;
                    ret.server = "Youtube";
                    ret.text = _txt;
                    ret.url = videoInfo.Url;
                    ret.typserveru = "video";

                    Console.WriteLine($"MP3 stream for {vid} DONE");
                    return ret;

                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR " + e.Message);
                    throw;
                }
                finally
                {
                    System.IO.File.Delete(tmpFile);
                }

            }
            return null;
        }


        class progress : IProgress<double>

        {
            string vid = "";
            public progress(string videoid)
            {
                this.vid = videoid;
            }
            double prevVal = 0;
            public void Report(double value)
            {
                double newVal = Math.Round(value, 2);
                if (newVal != prevVal)
                {
                    Console.WriteLine($"Converting video {this.vid} - {newVal:P3}");
                    prevVal = newVal;
                }
            }
        }




    }
}

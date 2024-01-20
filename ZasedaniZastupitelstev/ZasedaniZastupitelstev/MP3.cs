using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ZasedaniZastupitelstev
{
    public class MP3
    {
        public MP3(string mp3path, string apikey)
        {
            Mp3Path = mp3path;
            Apikey = apikey;
        }

        public string Mp3Path { get; }
        public string Apikey { get; }

        public void CheckAndDownload(string datasetid, string recordid, string videourl)
        {
            _checkDownloadAndStartV2T(false, datasetid, recordid, videourl);
        }

        public void CheckDownloadAndStartV2TOrGet(string datasetid, string recordid, string videourl)
        {
            _checkDownloadAndStartV2T(true, datasetid, recordid, videourl);
        }

        static HttpClient httpcl = new HttpClient();

        private void _checkDownloadAndStartV2T(bool startV2T, string datasetid, string recordid, string videourl)
        {

            string recId = recordid;
            string fnFile = $"{Mp3Path}\\{datasetid}\\{recId}";
            var MP3Fn = $"{fnFile}.mp3";
            var newtonFn = $"{fnFile}.mp3.raw_s2t";
            var dockerFn = $"{fnFile}.ctm";

            if (System.IO.File.Exists(MP3Fn) == false)
            {

                System.Diagnostics.ProcessStartInfo piv =
                            new System.Diagnostics.ProcessStartInfo("yt-dlp.exe",
                                $"--no-progress --extract-audio --audio-format mp3 --postprocessor-args \" -ac 1 -ar 16000\" -o \"{fnFile}.%(ext)s\" " + videourl
                                );
                Devmasters.ProcessExecutor pev = new Devmasters.ProcessExecutor(piv, 60 * 6 * 24);
                pev.StandardOutputDataReceived += (o, e) => { Program.logger.Debug($"Starting yt-dlp for {videourl} :" + e.Data); };

                Program.logger.Info("Starting yt-dlp download for {videourl} into {filename}",videourl,fnFile);
                pev.Start();
            }
            bool exists_S2T = System.IO.File.Exists(newtonFn) || System.IO.File.Exists(dockerFn);
            if (exists_S2T == false && startV2T)
            {
                Program.logger.Info("Starting Voice2Text from {filename} and rec {recId}", fnFile, recId);

                var localUrl = $"https://somedata.hlidacstatu.cz/mp3/{Program.DataSetId}/{recordid}.mp3";
                var respLocal = httpcl.GetAsync(localUrl, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                if (respLocal.IsSuccessStatusCode)
                {
                    var diarization = false;
                    var duration = Devmasters.SpeechToText.Audio.Util.AudioDurationSafe(new Uri(localUrl));
                    //if (duration.HasValue && duration.Value.TotalHours < 1.1)
                    //    diarization = true;

                    //Console.WriteLine(localUrl);
                    _ = Program.v2tApi.AddNewTaskAsync(
                        new HlidacStatu.DS.Api.Voice2Text.Options()
                        {
                            datasetName = Program.DataSetId,
                            itemId = recordid,
                            
                            diarization = diarization, 
                            language = "cs" 
                        },
                        new Uri(localUrl), Program.DataSetId, recordid, 2);

                }
            }

            /*
            if (exists_S2T)
            {

                if (System.IO.File.Exists(newtonFn))
                {
                    Program.logger.Info("Processing Newton converted Text from {filename} and rec {recId}", newtonFn, recId);
                    var tt = new Newton.SpeechToText.Cloud.FileAPI.VoiceToTerms(System.IO.File.ReadAllText(newtonFn));
                    blocks = new Devmasters.SpeechToText.VoiceToTextFormatter(tt.Terms)
                       .TextWithTimestamps(TimeSpan.FromSeconds(10), true);
                }
                else if (System.IO.File.Exists(dockerFn))
                {
                    Program.logger.Info("Processing docker converted Text from {filename} and rec {recId}", dockerFn, recId);
                    var tt = new KaldiASR.SpeechToText.VoiceToTerms(System.IO.File.ReadAllText(dockerFn));
                    blocks = new Devmasters.SpeechToText.VoiceToTextFormatter(tt.Terms)
                       .TextWithTimestamps(TimeSpan.FromSeconds(10), true);

                }
            }
            */
            //return blocks;

        }
    }
}

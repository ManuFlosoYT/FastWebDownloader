using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace FastWebDownloader
{
    public class Downloader
    {
        public static void ReadFile()
        {
            bool IsMusic = false;

            IEnumerable<string> lines = File.ReadLines(FWD.path);

            List<string> list = new List<string>(lines);

            list.RemoveAt(0);


            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Are you downloading music? (y/n) (invalid inputs will be considered as 'n'): ");
            char responseMusic = Console.ReadKey().KeyChar;
            if (responseMusic == 'y' || responseMusic == 'Y')
            {
                IsMusic = true;
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;

            //foreach element in list download it
            foreach (string str in list)
            {
                //if the string contains "playlist" download the playlist
                if (str.Contains("playlist"))
                {
                    DownloadPlaylist(str, IsMusic).Wait();
                }
                else
                {
                    Download(str, IsMusic).Wait();
                }
      
            }

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Do you want to wipe config.yaml? (y/n) (invalid inputs will be considered as 'n'): ");
            char response = Console.ReadKey().KeyChar;
            if (response == 'y' || response == 'Y')
            {
                File.Delete(FWD.path);
                FWD.CreateConfigFile();
            }
            Console.WriteLine();
            Console.WriteLine("Downloads finished, press any key to return . . .");
            Console.ReadKey();
            UI.MainMenu();
        }




        public static async Task Download(string url, bool IsMusic)
        {
            var ytdl = new YoutubeDL();
            ytdl.OutputFolder = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
            ytdl.YoutubeDLPath = FWD.YtDlppath;
            ytdl.FFmpegPath = FWD.FFmpegpath;



            var res = await ytdl.RunVideoDataFetch(url);
            VideoData video = res.Data;
            string title = video.Title;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Started downloading");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {title} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($" to ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"{ytdl.OutputFolder}");
            Console.WriteLine();

            var options = new OptionSet()
            {
                NoContinue = true,
                RestrictFilenames = true
            };

            if (IsMusic)
            {
                var resMusic = await ytdl.RunAudioDownload(
                    url,
                    AudioConversionFormat.Mp3
                );
            }
            else
            {
                var download = await ytdl.RunVideoDownload(
                    url,
                    overrideOptions: options
                );
            }

            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{title} finished downloading");
            Console.WriteLine();
        }

        public static async Task DownloadPlaylist(string url, bool IsMusic)
        {
            var ytdl = new YoutubeDL();
            ytdl.OutputFolder = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
            ytdl.YoutubeDLPath = FWD.YtDlppath;
            ytdl.FFmpegPath = FWD.FFmpegpath;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Downloading playlist, this could take a while...");
            Console.ForegroundColor = ConsoleColor.White;

            var options = new OptionSet()
            {
                Exec = "for %f in (*.mp3 *.wav *.flac *.ogg) do ffmpeg -i \"%f\" \"%~nf.mp3\""
            };


            if (IsMusic)
            {
                var res = await ytdl.RunAudioPlaylistDownload(
                    url,
                    overrideOptions: options
                );
            }
            else
            {
                var res = await ytdl.RunVideoPlaylistDownload(
                    url
                );
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Playlist finished downloading");
            Console.WriteLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using System.Diagnostics;
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

            Console.WriteLine();
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


            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Deleting temporal files...");
            Console.ForegroundColor = ConsoleColor.White;


            //foreach file in a directory remove spaces from the file name
            foreach (string filePath in Directory.EnumerateFiles(FWD.downloadsPath))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);

                //In the string the is a "[" and "]", delete everything between them and the brackets
                if (fileName.Contains("[") && fileName.Contains("]"))
                {
                    int start = fileName.IndexOf('[');
                    int end = fileName.IndexOf(']');
                    fileName = fileName.Remove(start, end - start + 1);
                }

                //Delete any non alphanumeric characters
                fileName = System.Text.RegularExpressions.Regex.Replace(fileName, "[^a-zA-Z0-9_]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);

                //If string has 2  or more consecutive underscores, remove one
                fileName = System.Text.RegularExpressions.Regex.Replace(fileName, "(_{2,})", "_", System.Text.RegularExpressions.RegexOptions.Compiled);
                
                //If the file name is the same as the new file name, skip it
                try
                {
                    File.Move(filePath, Path.Combine(FWD.downloadsPath, fileName + fileExtension));
                }
                catch (IOException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error renaming {fileName}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }


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
            Console.Write($"to ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"{ytdl.OutputFolder}");
            Console.WriteLine();

            var options = new OptionSet()
            {
                NoContinue = true,
                RestrictFilenames = true
            };

            var optionsMusic = new OptionSet()
            {
                NoContinue = true,
                RestrictFilenames = true,
                AudioFormat = AudioConversionFormat.Mp3
            };

            if (IsMusic)
            {
                var resMusic = await ytdl.RunAudioDownload(
                    url, 
                    overrideOptions: optionsMusic
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
                RestrictFilenames = true
            };

            var optionsMusic = new OptionSet()
            {
                RestrictFilenames = true,
                AudioFormat = AudioConversionFormat.Mp3
            };


            if (IsMusic)
            {
                var res = await ytdl.RunAudioPlaylistDownload(
                    url,
                    overrideOptions: optionsMusic
                );
            }
            else
            {
                var res = await ytdl.RunVideoPlaylistDownload(
                    url,
                    overrideOptions: options
                );
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Playlist finished downloading");
            Console.WriteLine();
        }
    }
}

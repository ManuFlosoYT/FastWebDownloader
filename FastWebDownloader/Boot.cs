using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FastWebDownloader
{
    public class FWD
    {
        public static string path = Path.Combine(Directory.GetCurrentDirectory(), "config.yaml");
        public static string downloadsPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
        public static string YtDlppath = Path.Combine(Directory.GetCurrentDirectory(), "yt-dlp.exe");
        public static string FFmpegpath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg.exe");


        public static void Main(string[] args)
        {
            EnsureLatestToolsAsync().Wait();
            CreateConfigFile();
            UI.MainMenu();
        }


        public static void CreateConfigFile()
        {
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("#Add one url per line without separators or spaces #When downloading a channel put the raw url https://www.youtube.com/@channel # DO NOT EDIT THIS FIRST LINE as this line is ignored in code");
                }
            }
        }

        public static async Task EnsureLatestToolsAsync()
        {
            await DownloadToolsAsync();
            await UpdateYtDlpAsync();
        }

        public static async Task DownloadToolsAsync()
        {
            if (!File.Exists(YtDlppath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Downloading YoutubeDLSharp Library, this could take some time...");
                Console.ForegroundColor = ConsoleColor.White;
                await YoutubeDLSharp.Utils.DownloadYtDlp();
            }
            if (!File.Exists(FFmpegpath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Downloading FFmpeg Library, this could take some time...");
                Console.ForegroundColor = ConsoleColor.White;
                await YoutubeDLSharp.Utils.DownloadFFmpeg();
            }
        }

        private static async Task UpdateYtDlpAsync()
        {
            try
            {
                if (File.Exists(YtDlppath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = YtDlppath,
                        Arguments = "-U",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Directory.GetCurrentDirectory()
                    };
                    using (var p = new Process { StartInfo = psi })
                    {
                        p.Start();
                        await Task.Run(() => p.WaitForExit());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Warning: yt-dlp update failed: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
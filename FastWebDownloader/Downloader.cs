using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                else if (str.Contains("youtube.com/@"))
                {
                    // Normaliza URLs de canal para apuntar a la pestaña de vídeos
                    var normalized = str;
                    if (!normalized.Contains("/videos") && !normalized.Contains("/playlists") && !normalized.Contains("/streams") && !normalized.Contains("/shorts"))
                    {
                        normalized = normalized.TrimEnd('/') + "/videos";
                    }
                    DownloadPlaylist(normalized, IsMusic).Wait();
                }
                else
                {
                    Download(str, IsMusic).Wait();
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
                RestrictFilenames = true,
                // Evita formatos no disponibles por SABR con una cadena de fallback
                Format = "bv*+ba/b/best"
            };

            var optionsMusic = new OptionSet()
            {
                NoContinue = true,
                RestrictFilenames = true,
                AudioFormat = AudioConversionFormat.Mp3,
                // Asegura que se elija el mejor audio disponible si falta el formato por defecto
                Format = "bestaudio/best"
            };

            if (IsMusic)
            {
                var resMusic = await ytdl.RunAudioDownload(
                    url, 
                    overrideOptions: optionsMusic
                );
                if (!resMusic.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Join(Environment.NewLine, resMusic.ErrorOutput ?? new string[0]));
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                var download = await ytdl.RunVideoDownload(
                    url,
                    overrideOptions: options
                );
                if (!download.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Join(Environment.NewLine, download.ErrorOutput ?? new string[0]));
                    Console.ForegroundColor = ConsoleColor.White;
                }
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

            // Asegura que la carpeta exista
            Directory.CreateDirectory(ytdl.OutputFolder);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Downloading playlist, this could take a while...");
            Console.ForegroundColor = ConsoleColor.White;

            var options = new OptionSet()
            {
                RestrictFilenames = false,
                // Fallback de formatos para reducir "Requested format is not available"
                Format = "bv*+ba/b/best"
            };

            var optionsMusic = new OptionSet()
            {
                RestrictFilenames = false,
                AudioFormat = AudioConversionFormat.Mp3,
                // Mejor audio disponible
                Format = "bestaudio/best"
            };

            // 1) Intentar obtener metadata de la playlist para conocer el total de vídeos
            try
            {
                var metaRes = await ytdl.RunVideoDataFetch(url, flat: false);
                if (metaRes != null && metaRes.Success && metaRes.Data != null && metaRes.Data.Entries != null)
                {
                    var entries = metaRes.Data.Entries.ToList();
                    int total = entries.Count;

                    if (total > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Found {total} videos in playlist.");
                        Console.ForegroundColor = ConsoleColor.White;

                        int index = 0;
                        foreach (var entry in entries)
                        {
                            index++;
                            string entryUrl = !string.IsNullOrEmpty(entry.WebpageUrl) ? entry.WebpageUrl : entry.Url;
                            if (string.IsNullOrWhiteSpace(entryUrl))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[{index}/{total}] Skipped: invalid entry URL.");
                                Console.ForegroundColor = ConsoleColor.White;
                                continue;
                            }

                            string title = !string.IsNullOrEmpty(entry.Title) ? entry.Title : entry.ID;

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"[{index}/{total}] Started downloading");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write($" {title} ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"to ");
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write($"{ytdl.OutputFolder}");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;

                            try
                            {
                                if (IsMusic)
                                {
                                    var r = await ytdl.RunAudioDownload(
                                        entryUrl,
                                        overrideOptions: optionsMusic
                                    );
                                    if (!r.Success)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"[{index}/{total}] Failed downloading {title}:\n{string.Join(Environment.NewLine, r.ErrorOutput ?? new string[0])}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        continue;
                                    }
                                }
                                else
                                {
                                    var r = await ytdl.RunVideoDownload(
                                        entryUrl,
                                        overrideOptions: options
                                    );
                                    if (!r.Success)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"[{index}/{total}] Failed downloading {title}:\n{string.Join(Environment.NewLine, r.ErrorOutput ?? new string[0])}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        continue;
                                    }
                                }

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"[{index}/{total}] {title} finished downloading");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[{index}/{total}] Failed downloading {title}: {ex.Message}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Playlist finished downloading ({total} videos).");
                        Console.WriteLine();
                        return;
                    }
                }
            }
            catch
            {
                // Si falla la obtención de metadata, continuamos con el modo por lotes
            }

            // 2) Fallback al comportamiento anterior (descarga por comando de playlist completo)
            if (IsMusic)
            {
                var resAudio = await ytdl.RunAudioPlaylistDownload(
                    url,
                    overrideOptions: optionsMusic
                );
                if (!resAudio.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Playlist download failed:\n" + string.Join(Environment.NewLine, resAudio.ErrorOutput ?? new string[0]));
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
            }
            else
            {
                var resVideo = await ytdl.RunVideoPlaylistDownload(
                    url,
                    overrideOptions: options
                );
                if (!resVideo.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Playlist download failed:\n" + string.Join(Environment.NewLine, resVideo.ErrorOutput ?? new string[0]));
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Playlist finished downloading");
            Console.WriteLine();
        }
    }
}

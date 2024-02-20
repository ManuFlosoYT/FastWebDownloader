using System;
using System.Diagnostics;

namespace FastWebDownloader
{
    public class UI
    {
        public static void MainMenu()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\r\n                                          \r\n                                          \r\n    ,---,.            .---.     ,---,     \r\n  ,'  .' |           /. ./|   .'  .' `\\   \r\n,---.'   |       .--'.  ' ; ,---.'     \\  \r\n|   |   .'      /__./ \\ : | |   |  .`\\  | \r\n:   :  :    .--'.  '   \\' . :   : |  '  | \r\n:   |  |-, /___/ \\ |    ' ' |   ' '  ;  : \r\n|   :  ;/| ;   \\  \\;      : '   | ;  .  | \r\n|   |   .'  \\   ;  `      | |   | :  |  ' \r\n'   :  '     .   \\    .\\  ; '   : | /  ;  \r\n|   |  |      \\   \\   ' \\ | |   | '` ,/   \r\n|   :  \\       :   '  |--\"  ;   :  .'     \r\n|   | ,'        \\   \\ ;     |   ,.'       \r\n`----'           '---\"      '---'         \r\n                                          \r\n");
            Console.ForegroundColor = ConsoleColor.White;


            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("1. Download videos");
            Console.WriteLine("2. Proyect GitHub");
            Console.WriteLine("3. Exit");
            Console.ForegroundColor = ConsoleColor.White;


            string input = Console.ReadLine();
            if (int.TryParse(input, out int choice))
            {
                if (choice < 1 || choice > 3)
                {
                    MainMenu();
                }
            }
            else
            {
                MainMenu();
            }


            switch (choice)
            {
                case 1:
                    Downloader.ReadFile();
                    break;
                case 2:
                    string url = "https://github.com/ManuFlosoYT/FastWebDownloader";
                    Process.Start(url);
                    break;
                case 3:
                    Environment.Exit(0);
                    break;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
        }
    }
}

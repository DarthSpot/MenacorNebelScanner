using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MenacorNebelScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                PrintHelp();
            }
            else
            {
                var arguments = args.Select((x, i) => (x, i)).ToList();
                if (!args.Contains("-f") || !args.Contains("-t") || !args.Contains("-o") || args.Length != 6)
                    PrintHelp();
                else
                {
                    var f = int.Parse(args[arguments.Single(x => x.x == "-f").i + 1]);
                    var t = int.Parse(args[arguments.Single(x => x.x == "-t").i + 1]);
                    var o = args[arguments.Single(x => x.x == "-o").i + 1];         
                    var scanner = new MenacorScanner();
                    scanner.OnUpdateProgress += OnUpdate;
                    WriteData(scanner.Scan(f, t), o);
                }
            }
        }

        private static void OnUpdate(object sender, int counter)
        {
            Console.Write($"\rSeite {counter} wird gelesen...");
        }

        private static void WriteData(List<Post> scan, string fileName)
        {
            var header = "ID;User;TimeStamp;PostLength";
            File.WriteAllLines(fileName, scan.Select(x => x.ToString()).Prepend(header));
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Willkommen bei Menacors Nebelscanner!");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Aufruf:");
            Console.WriteLine("menacor -f <ForumID> -t <ThreadID> -o <Ausgabedatei>");
            Console.WriteLine("Beispiel: \"menacor -f 13 -t 44833 -o sternenlosenacht.txt");
        }
    }
}
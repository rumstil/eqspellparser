using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace parser
{
    class Program
    {
        static void Main(string[] args)
        {
            const string spellFile = "spells_us.txt";
            const string descFile = "dbstr_us.txt";

            if (!File.Exists(spellFile))
                DownloadPatchFile(spellFile);

            if (!File.Exists(descFile))
                DownloadPatchFile(descFile);

            List<Spell> list = SpellParser.LoadFromFile(spellFile, descFile);

            if (args.Length > 0)
            {
                Search(list, args);
            }
            else
            {
                while (true)
                {
                    Console.Error.WriteLine();
                    Console.Error.Write("parser>");
                    string cmd = Console.ReadLine().Trim().ToLower();
                    if (cmd == "help" || cmd == "")
                        Help();
                    if (cmd == "quit")                    
                        break;
                    //if (String.IsNullOrEmpty(cmd))
                    Search(list, cmd.Split());
                };
            }
        }

        static void Help()
        {
            Console.Error.WriteLine("USAGE EXAMPLES:");
            Console.Error.WriteLine("parser all");
            Console.Error.WriteLine("parser name \"complete heal\" ");
            Console.Error.WriteLine("parser id 13");
            Console.Error.WriteLine("parser type 3");
            Console.Error.WriteLine("parser class clr");
        }

        /// <summary>
        /// Search the spell list for matching spells
        /// </summary>
        static void Search(List<Spell> list, string[] args)
        {
            IEnumerable<Spell> results = null;

            if (args.Length == 1 && args[0] == "all")
                results = list;

            // search by effect type
            if (args.Length == 2 && args[0] == "type")
                results = list.Where(x => x.DebugEffectList.Contains(";" + args[1] + ";"));

            // search by id
            if (args.Length == 2 && args[0] == "id")
                results = list.Where(x => x.ID.ToString() == args[1]);

            // search by name
            if (args.Length == 2 && args[0] == "name")
                results = list.Where(x => x.Name.ToLower().Contains(args[1].ToLower())).OrderBy(x => x.Name);

            // search by class
            if (args.Length == 2 && args[0] == "class")
            {
                int i = (int)Enum.Parse(typeof(SpellClasses), args[1].ToUpper()) - 1;
                results = list.Where(x => x.Levels[i] > 0 && x.Levels[i] < 255).OrderBy(x => x.Levels[i]).ThenBy(x => x.ID);
            }

            // show results
            if (results != null)
                foreach (Spell spell in results)
                    Console.WriteLine("\r\n{0}\r\n{1}", spell, String.Join("\r\n", spell.Details()));

            //Console.WriteLine("============================================================================");
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        static void DownloadFile(string url, string path)
        {
            using (WebClient web = new WebClient())
            {
                byte[] data = web.DownloadData(url);

                Stream datastream = new MemoryStream(data);
                GZipStream input = new GZipStream(datastream, CompressionMode.Decompress);
                using (FileStream output = new FileStream(path, FileMode.Create))
                {
                    byte[] buffer = new byte[32768];
                    while (true)
                    {
                        int read = input.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                            return;
                        output.Write(buffer, 0, read);
                    }
                }
            }
        }

        /// <summary>
        /// Download a file from the patch server.
        /// </summary>
        static void DownloadPatchFile(string filename)
        {
            Console.Error.WriteLine("Downloading " + filename);

            // http://eqitems.13th-floor.org/phpBB2/viewtopic.php?t=316
            // Newest Patcher path: http://patch.everquest.com:7000/patch/lp2/eq/en/patch1/en-main/
            // Older Patcher path: http://patch.everquest.com:7000/patch/everquest/en/patch1/main/
            // Looking at the patcher logs it seems they are switching back and forth between patch0 and patch1. 
            DownloadFile("http://patch.station.sony.com:7000/patch/everquest/en/patch1/main/" + filename + ".gz", filename);
        }

    }
}

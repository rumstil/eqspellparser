using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.IO.Compression;

namespace parser
{
    class Program
    {
        static void Main(string[] args)
        {
            const string path = "spells_us.txt";

            // http://eqitems.13th-floor.org/phpBB2/viewtopic.php?t=316
            // Newest Patcher path: http://patch.everquest.com:7000/patch/lp2/eq/en/patch1/en-main/
            // Older Patcher path: http://patch.everquest.com:7000/patch/everquest/en/patch1/main/
            // Looking at the patcher logs it seems they are switching back and forth between patch0 and patch1. 
            if (!File.Exists(path))
                DownloadFile("http://patch.station.sony.com:7000/patch/everquest/en/patch1/main/spells_us.txt.gz", path);

            IEnumerable<Spell> list = SpellParser.LoadFromFile(path);
            IEnumerable<Spell> results = null;
            
            Console.WriteLine("============================================================================");

            if (args.Length == 0)
                results = list;

            // search by effect type
            if (args.Length == 1)
                results = list.Where(x => x.DebugEffectList.Contains(";" + args[0] + ";"));
            
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
                foreach(Spell spell in results)
                    Console.WriteLine("\r\n{0}\r\n{1}", spell, String.Join("\r\n", spell.Details().ToArray()));

        }

        /// <summary>
        /// Download and decompress a file.
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

    }
}

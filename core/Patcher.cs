using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
// Launchpad files are all LZMA compressed. I've included it as binary for easier building of this project.
// SevenZip (lzma.dll) is compiled from v9.20 of the LZMA SDK http://www.7-zip.org/sdk.html
using SevenZip;
using System.Globalization;


namespace EQSpellParser
{
    public static class LaunchpadPatcher
    {
        /// <summary>
        /// Download all spell files from the patch server and include version info in filenames.
        /// </summary>
        /// <param name="server">null for the live servers. "-test" for test server.</param>
        public static string DownloadSpellFilesWithVersioning(string server = null, string path = null)
        {
            // Path.Combine doesn't like null paths
            if (path == null)
                path = "";

            DownloadManifest(server, "manifest.dat");
            var manifest = new LaunchpadManifest(File.ReadAllBytes("manifest.dat"));

            var spell = manifest.FindFile(LaunchpadManifest.SPELL_FILE);
            if (spell == null)
                throw new InvalidOperationException("Could not locate main spell file in manifest.dat.");

            var version = "-" + spell.LastModified.ToString("yyyy-MM-dd") + server;

            spell.Name = spell.Name.Replace(".txt", version + ".txt");
            DownloadFile(spell.Url, Path.Combine(path, spell.Name));

            // the other files can sometimes be older than the spell file so we always save them with the main files date
            var desc = manifest.FindFile(LaunchpadManifest.SPELLDESC_FILE);
            if (desc != null)
            {
                desc.Name = desc.Name.Replace(".txt", version + ".txt");
                DownloadFile(desc.Url, Path.Combine(path, desc.Name));
            }

            var spellstr = manifest.FindFile(LaunchpadManifest.SPELLSTR_FILE);
            if (spellstr != null)
            {
                spellstr.Name = spellstr.Name.Replace(".txt", version + ".txt");
                DownloadFile(spellstr.Url, Path.Combine(path, spellstr.Name));
            }

            var stack = manifest.FindFile(LaunchpadManifest.SPELLSTACK_FILE);
            if (stack != null)
            {
                stack.Name = stack.Name.Replace(".txt", version + ".txt");
                DownloadFile(stack.Url, Path.Combine(path, stack.Name));

            }

            return Path.Combine(path, spell.Name);
        }

        /// <summary>
        /// Download all spell files from the patch server. 
        /// </summary>
        public static string DownloadSpellFiles(string server = null, string path = null)
        {
            // Path.Combine doesn't like null paths
            if (path == null)
                path = "";

            DownloadManifest(server, "manifest.dat");
            var manifest = new LaunchpadManifest(File.ReadAllBytes("manifest.dat"));

            var spell = manifest.FindFile(LaunchpadManifest.SPELL_FILE);
            if (spell == null)
                throw new InvalidOperationException("Could not locate main spell file in manifest.dat.");

            DownloadFile(spell.Url, Path.Combine(path, spell.Name));

            var desc = manifest.FindFile(LaunchpadManifest.SPELLDESC_FILE);
            if (desc != null)
            {
                DownloadFile(desc.Url, Path.Combine(path, desc.Name));
            }

            var spellstr = manifest.FindFile(LaunchpadManifest.SPELLSTR_FILE);
            if (spellstr != null)
            {
                DownloadFile(spellstr.Url, Path.Combine(path, spellstr.Name));
            }

            var stack = manifest.FindFile(LaunchpadManifest.SPELLSTACK_FILE);
            if (stack != null)
            {
                DownloadFile(stack.Url, Path.Combine(path, stack.Name));
            }

            return spell.Name;
        }

        /// <param name="server">null for the live servers. "-test" for test server.</param>
        public static void DownloadManifest(string server, string saveToPath)
        {
            string url = String.Format("http://manifest.patch.daybreakgames.com/patch/sha/manifest/eq/eq-en{0}/live/eq-en{0}.sha.soe", server);
            DownloadFile(url, saveToPath);
        }

        public static void DownloadFile(string url, string saveToPath)
        {
            Console.Error.WriteLine("=> " + url);

            using (WebClient web = new WebClient())
            {
                web.Headers["User-Agent"] = "Quicksilver Player/1.0.3.183";
                web.Proxy = null;
                byte[] data = web.DownloadData(url);
                Stream inStream = new MemoryStream(data);
                File.Delete(saveToPath);
                using (FileStream outStream = new FileStream(saveToPath, FileMode.Create))
                {
                    // Launchpad files are LZMA compressed
                    Decompress(inStream, outStream);
                    Console.Error.WriteLine("   {2} [{0:#,#} bytes] {1}", outStream.Length, web.ResponseHeaders["Last-Modified"], saveToPath);
                    Console.Error.WriteLine();
                }

                // this timestamp may be different than what was reported in the manifest
                DateTime lastMod;
                if (DateTime.TryParse(web.ResponseHeaders["Last-Modified"], CultureInfo.InvariantCulture, DateTimeStyles.None, out lastMod))
                {
                    File.SetCreationTime(saveToPath, lastMod);
                    File.SetLastWriteTimeUtc(saveToPath, lastMod);
                }
            }
        }

        private static void Decompress(Stream inStream, Stream outStream)
        {
            byte[] properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw new Exception("Input .lzma is too short");

            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            decoder.SetDecoderProperties(properties);
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = inStream.ReadByte();
                outSize |= ((long)(byte)v) << (8 * i);
            }
            long compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
        }

    }

    /// <summary>
    /// The launchpad manifest is basically a binary directory structure with some meta data. 
    /// It contains a list of all the files needed to run the game, download URLs and local path names.
    /// Before we download any files we need to look them up in the manifest to get the URLs for them.
    /// The only file with a fixed URL is the manifest itself.
    /// </summary>
    public class LaunchpadManifest
    {
        public const string SPELL_FILE = "spells_us.txt";
        public const string SPELLSTR_FILE = "spells_us_str.txt";
        public const string SPELLDESC_FILE = "dbstr_us.txt";
        public const string SPELLSTACK_FILE = "SpellStackingGroups.txt";

        public class FileInfo
        {
            public string Name;
            public string Url;
            public long CompressedSize;
            public long UncompressedSize;
            public DateTime LastModified;
            public long CRC32;

            public override string ToString()
            {
                return String.Format("{0} Size:{1} Modified:{2} Url:{3}", Name, UncompressedSize, LastModified.ToString("yyyy-MM-dd"), Url);
            }
        }

        // keep 2 refernces to the manifest byte stream
        // binary for reading
        private Stream data;
        // string for searching
        private string text;

        public LaunchpadManifest(byte[] buffer)
        {
            data = new MemoryStream(buffer);
            text = Encoding.ASCII.GetString(buffer);
        }

        public LaunchpadManifest(Stream stream)
        {
            data = stream;

            StreamReader str = new StreamReader(data, Encoding.ASCII);
            text = str.ReadToEnd();
        }

        public FileInfo FindFile(string name)
        {
            string root = "http://eq.patch.daybreakgames.com/patch/sha/eq/eq.sha.zs";

            // set file position to the location of the filename
            // if any of the other important attributes preceeded the filename (as they can) then this will fail to load them 
            int pos = text.IndexOf(name) - 2;
            if (pos <= 0)
            {
                Console.Error.WriteLine("Could not find {0} in manifest.", name);
                return null;
            }
            data.Position = pos;
            FileInfo file = ReadFile();
            file.Url = root + "/" + file.Url;
            return file;
        }

        public List<FileInfo> Files()
        {
            throw new NotImplementedException();

            List<FileInfo> list = new List<FileInfo>();

            // 2015-7-22 the parser broke so rather than trying to read the entire manifest i'm just going to look 
            // for the few files i need

            /*

            using (Stream f = File.OpenRead(path))
            {

                // i'm not sure how to decode the header so i will try to skip to the first known folder entry
                // this may break in the future but it's fast and easy to do for now.
                // converting a byte array to an ascii string should be ok since alpha numeric chars will not be maligned.
                // an alternative would be to base16 encode both and then do the search
                byte[] header = new byte[2048];
                f.Read(header, 0, header.Length);
                f.Position = Encoding.ASCII.GetString(header).IndexOf("ActorEffects") - 5;

                //f.Position = 578; //  first folder

                while (f.Position < f.Length)
                {
                    int type = f.ReadByte();
                    //Console.Write(f.Position.ToString() + " - " + type + " ");

                    // subfolder. if folders are nested, several of these will be chained together
                    // i'm just ignoring these since local paths not needed for the spell parser
                    if (type == 2)
                    {
                        int t1 = f.ReadByte();
                        if (t1 == 255)
                            f.Position += 3;
                        if (t1 >= 128)
                            f.Position += 1;

                        string name = ReadString(f);

                        //Console.WriteLine(name);
                    }

                    // end of folder chain. file list follows
                    if (type == 254)
                    {
                        f.Position += 4;
                    }

                    // file in current folder
                    if (type == 3)
                    {
                        FileInfo file = ReadFile(f);
                        file.Url = root + "/" + file.Url;
                        files.Add(file);
                    }

                    if (type == 40)
                    {
                        //root = readString();
                    }

                    //Console.ReadLine();
                }
                 
            }
            */

            return list;
        }

        private FileInfo ReadFile()
        {
            FileInfo file = new FileInfo();

            //int size = ReadSize(f);
            //long next = f.Position + size;

            // each file record contains the following attributes 
            // 1 = file name
            // 2 = compressed size
            // 3 = uncompressed size
            // 4 = crc32
            // 6 = delete file? 060101
            // 8 = timestamp in unix time
            // 18 = hash of some sort (forms the URL)

            while (true)
            {
                int type = data.ReadByte();

                if (type == 1)
                    file.Name = ReadString();
                else if (type == 2)
                    file.CompressedSize = ReadInt();
                else if (type == 3)
                    file.UncompressedSize = ReadInt();
                else if (type == 4)
                    file.CRC32 = ReadInt();
                else if (type == 8)
                    file.LastModified = ReadDateTime();
                else if (type == 10)
                {
                    // no idea what type 10 is
                    int len = ReadSize();
                    data.Position += len;
                }
                else if (type == 18)
                {
                    int len = data.ReadByte();
                    byte[] hash = new byte[len];
                    data.Read(hash, 0, len);
                    file.Url = EncodeAsBase16String(hash).Insert(5, "/").Insert(2, "/");
                }
                else break;
            }

            //f.Position = next;
            //byte[] buf = new byte[next - f.Position];
            //f.Read(buf, 0, buf.Length);
            //Console.Error.WriteLine(file);
            return file;
        }

        /// <summary>
        /// Read a datetime from the manifest.
        /// </summary>
        private DateTime ReadDateTime()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(ReadInt());
        }

        /// <summary>
        /// Read an integer from the manifest. Integers are stored as a variable length byte array.
        /// </summary>
        private int ReadInt()
        {
            int len = data.ReadByte();
            int result = 0;
            for (int i = 0; i < len; i++)
                result = (result << 8) + data.ReadByte();
            return result;
        }

        /// <summary>
        /// Read a string from the manifest.
        /// </summary>
        private string ReadString()
        {
            int len = data.ReadByte();
            byte[] buf = new byte[len];
            data.Read(buf, 0, len);
            // not sure if the encoding is actually UTF8 
            return Encoding.UTF8.GetString(buf).TrimEnd('\0');
        }

        /// <summary>
        /// Read size of the next variable length field. For some reason even the "size" itself is variable length.
        /// </summary>
        private int ReadSize()
        {
            int size = data.ReadByte();

            if (size == 255)
            {
                // size is an int32
                size = 0;
                for (int i = 0; i < 4; i++)
                    size = (size << 8) + data.ReadByte();
            }
            else if (size >= 128)
            {
                // size is an int16
                size = ((size & 0x7F) << 8) + data.ReadByte();
            }

            return size;
        }

        private static string EncodeAsBase16String(byte[] bytes)
        {
            StringBuilder s = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                s.Append(b.ToString("x2"));
            return s.ToString();
        }
    }



}

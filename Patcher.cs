using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
// SevenZip (lzma.dll) is compiled from v9.20 of the LZMA SDK http://www.7-zip.org/sdk.html
// I've included it as binary for easier building of this project.
using SevenZip;

namespace Everquest
{
    // .net 2.0 doesn't include the standard Func() delegates
    //public delegate TResult Func<TResult>();

    public class LaunchpadPatcher
    {
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

        /// <summary>
        /// The manifest is basically a binary directory structure with some meta data. 
        /// The URL for this is static. We need to download this to find the dynamic URLs for all other patch files.
        /// </summary>
        /// <param name="server">null for the live servers. "-test" for test server.</param>
        public static void DownloadManifest(string server, string path)
        {
            string url = String.Format("http://manifest.patch.station.sony.com/patch/sha/manifest/eq/eq-en{0}/live/eq-en{0}.sha.soe", server);
            DownloadFile(url, path);
        }

        public static List<FileInfo> LoadManifest(string path)
        {
            string root = "http://eq.patch.station.sony.com/patch/sha/eq/eq.sha.zs";

            List<FileInfo> files = new List<FileInfo>();

            // 2015-7-22 the parser broke so rather than trying to read the entire manifest i'm just going to look 
            // for the 2 files i need

            //string text = Encoding.ASCII.GetString(File.ReadAllBytes(path));
            using (Stream f = File.OpenRead(path))
            {
                StreamReader str = new StreamReader(f, Encoding.ASCII);
                string text = str.ReadToEnd();

                f.Position = text.IndexOf("spells_us.txt") - 4;
                FileInfo file = ReadFile(f);
                file.Url = root + "/" + file.Url;
                files.Add(file);

                f.Position = text.IndexOf("dbstr_us.txt") - 4;
                file = ReadFile(f);
                file.Url = root + "/" + file.Url;
                files.Add(file);
            }



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

            return files;
        }

        private static FileInfo ReadFile(Stream f)
        {
            FileInfo file = new FileInfo();

            //int size = f.ReadByte();
            //if (size >= 128)
            //    size = ((size & 0xF) << 8) + f.ReadByte();

            int size = ReadSize(f);
            long next = f.Position + size;

            // each file record contains the following attributes 
            // 1 = file name
            // 2 = compressed size
            // 3 = uncompressed size
            // 4 = crc32
            // 6 = delete file? 060101
            // 8 = timestamp in unix time
            // 18 = hash of some sort (forms the URL)

            while (f.Position < next)
            {
                int type = f.ReadByte();

                if (type == 1)
                    file.Name = ReadString(f);
                else if (type == 2)
                    file.CompressedSize = ReadInt(f);
                else if (type == 3)
                    file.UncompressedSize = ReadInt(f);
                else if (type == 4)
                    file.CRC32 = ReadInt(f);
                else if (type == 8)
                    file.LastModified = ReadDateTime(f);
                else if (type == 10)
                {
                    // no idea what this is
                    int len = ReadSize(f);
                    f.Position += len;
                }
                else if (type == 18)
                {
                    int len = f.ReadByte();
                    byte[] hash = new byte[len];
                    f.Read(hash, 0, len);
                    file.Url = EncodeAsBase16String(hash).Insert(5, "/").Insert(2, "/");
                }
                else break;
            }

            f.Position = next;
            //byte[] buf = new byte[next - f.Position];
            //f.Read(buf, 0, buf.Length);
            //Console.Error.WriteLine(file);
            return file;
        }

        /// <summary>
        /// Read a datetime from the manifest.
        /// </summary>
        private static DateTime ReadDateTime(Stream f)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(ReadInt(f));
        }

        /// <summary>
        /// Read an integer from the manifest.
        /// </summary>
        private static int ReadInt(Stream f)
        {
            int len = f.ReadByte();
            int result = 0;
            for (int i = 0; i < len; i++)
                result = (result << 8) + f.ReadByte();
            return result;
        }

        /// <summary>
        /// Read a string from the manifest.
        /// </summary>
        private static string ReadString(Stream f)
        {
            int len = f.ReadByte();
            byte[] buf = new byte[len];
            f.Read(buf, 0, len);
            return Encoding.UTF8.GetString(buf).TrimEnd('\0');
        }

        /// <summary>
        /// Read size of the next variable length field. For some reason even the "size" itself is variable length.
        /// </summary>
        private static int ReadSize(Stream f)
        {
            int size = f.ReadByte();

            if (size == 255)
            {
                // size is an int32
                size = 0;
                for (int i = 0; i < 4; i++)
                    size = (size << 8) + f.ReadByte();
            }
            else if (size >= 128)
            {
                // size is an int16
                size = ((size & 0x7F) << 8) + f.ReadByte();
            }

            return size;
        }

        public static void DownloadFile(string url, string path)
        {
            Console.Error.WriteLine("=> " + url);

            using (WebClient web = new WebClient())
            {
                web.Headers["User-Agent"] = "Quicksilver Player/1.0.3.183";
                web.Proxy = null;
                byte[] data = web.DownloadData(url);
                Stream inStream = new MemoryStream(data);
                using (FileStream outStream = new FileStream(path, FileMode.Create))
                {
                    // Launchpad files are LZMA compressed
                    Decompress(inStream, outStream);
                    Console.Error.WriteLine("   {2} [{0} bytes] {1}", outStream.Length, web.ResponseHeaders["Last-Modified"], path);
                    Console.Error.WriteLine();
                }

                DateTime lastMod;
                if (DateTime.TryParse(web.ResponseHeaders["Last-Modified"], out lastMod))
                    File.SetLastWriteTimeUtc(path, lastMod);
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

        private static string EncodeAsBase16String(byte[] bytes)
        {
            StringBuilder s = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                s.Append(b.ToString("x2"));
            return s.ToString();
        }


    }

}

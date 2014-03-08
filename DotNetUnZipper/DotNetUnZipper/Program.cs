using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace DotNetUnZipper
{
    class Program
    {
        static void Main(string[] args)
        {
            var dirPath = @"c:\test\testZips";
            var di = new DirectoryInfo(dirPath);
            var files = di.GetFiles("*.zip");
            Parallel.ForEach(files, currentfile =>
            {
                Extract(currentfile);
                Console.WriteLine("Decompressing {0} on thread {1}", currentfile, Thread.CurrentThread.ManagedThreadId);
            });
        }

        public static void Decompress(FileInfo file)
        {
            using (var inFile = file.OpenRead())
            {
                var curFile = file.FullName;
                var origName = curFile.Remove(curFile.Length - file.Extension.Length);

                using (var outfile = File.Create(origName))
                {

                    using (var decompress = new GZipStream(inFile, CompressionMode.Decompress))
                    {
                        decompress.CopyTo(outfile);
                    }
                }
            }
        }

        public static void Extract(FileInfo file)
        {
            string outfile = @"c:\test\unzipped";
            ZipFile zf = null;
            try
            {
                using (var fs = file.OpenRead())
                {
                    zf = new ZipFile(fs);
                    foreach (ZipEntry entry in zf)
                    {
                        if (!entry.IsFile)
                        {
                            continue;
                        }
                        var entryFileName = entry.Name;
                        var buffer = new byte[4096];
                        var zipStream = zf.GetInputStream(entry);
                        var fullZipToPath = Path.Combine(outfile, entryFileName);
                        var directoryName = Path.GetDirectoryName(fullZipToPath);

                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                }
                zf.Close();
            }
        }

        
    }
}

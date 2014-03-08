using System;
using System.Collections.Generic;
using System.Configuration;
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
            var dirPath = ConfigurationSettings.AppSettings["SourceDirectory"];
            var di = new DirectoryInfo(dirPath);
            var files = di.GetFiles("*.zip");
            Parallel.ForEach(files, currentfile =>
            {
                Extract(currentfile);
                Console.WriteLine("Decompressing {0} on thread {1}", currentfile, Thread.CurrentThread.ManagedThreadId);
            });
        }
        private static void Extract(FileInfo file)
        {
            var outfile = ConfigurationSettings.AppSettings["DestinationDirectory"];
            ZipFile zipFile = null;
            try
            {
                using (var fs = file.OpenRead())
                {
                    zipFile = new ZipFile(fs);
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue;
                        }
                        var entryFileName = zipEntry.Name;
                        var buffer = new byte[4096];
                        var zipStream = zipFile.GetInputStream(zipEntry);
                        var fullZipToPath = Path.Combine(outfile, entryFileName);
                        var directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }
                        using (var streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
            }
            finally
            {
                if (zipFile != null)
                {
                    zipFile.IsStreamOwner = true;
                    zipFile.Close();
                }
            }
        }
    }
}

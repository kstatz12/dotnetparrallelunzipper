using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Configuration;
namespace DotNetUnZipper
{
    static class Program
    {
        static readonly string DirPath = ConfigurationSettings.AppSettings["SourceDirectory"];
        static readonly string ArchivePath = ConfigurationSettings.AppSettings["ArchiveDirectory"];
        static void Main(string[] args)
        {
            try
            {
                var di = new DirectoryInfo(DirPath);
                var files = di.GetFiles("*.zip");

                if (files.Length > 0)
                {
                    Parallel.ForEach(files, info =>
                    {
                        bool status;
                        //info.CopyTo(ArchivePath, false);
                        //Console.WriteLine("Archived {0} Successfully", info.Name);
                        Extract(info, out status);
                        if (status)
                        {
                            Console.WriteLine("Unzipped {0} Successfully", info.Name);
                            info.Delete();
                            Console.WriteLine("Deleted {0} Successfully", info.Name);
                        }
                        else
                        {
                            Archive(info);
                            Console.WriteLine("Moved Failed Unzips to Archive {0}", info.Name);
                        }
                        
                    });
                }
                else
                {
                    throw new Exception("No Zip Files Found In That Directory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        private static void Archive(FileInfo file)
        {
            if (file != null)
            {
                var path = Path.Combine(ArchivePath, file.Name);
                file.CopyTo(path, true);
                file.Delete();
            }
        }
        private static void Extract(FileInfo file, out bool status)
        {
            bool successStatus = false;
            var outfile = ConfigurationSettings.AppSettings["DestinationDirectory"];
            ZipFile zipFile = null;
            try
            {
                using (var fs = file.OpenRead())
                {
                    zipFile = new ZipFile(fs);

                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        if (zipEntry.IsFile)
                        {
                            status = false;
                            throw new Exception("No File Found");

                        }
                        var entryFileName = zipEntry.Name;
                        entryFileName = Path.GetFileName(entryFileName);
                        var buffer = new byte[4096];
                        var zipStream = zipFile.GetInputStream(zipEntry);
                        if (entryFileName != null)
                        {
                            var fullZipToPath = Path.Combine(outfile, entryFileName);
                            var directoryName = Path.GetDirectoryName(fullZipToPath);
                            if (!string.IsNullOrEmpty(directoryName))
                            {
                                Directory.CreateDirectory(directoryName);
                            }
                            
                            using (var streamWriter = File.Create(fullZipToPath))
                            {
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                                successStatus = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                successStatus = false;
            }
            finally
            {
                if (zipFile != null)
                {
                    zipFile.IsStreamOwner = true;
                    zipFile.Close();
                    status = successStatus;
                }
                status = successStatus;
            }
        }
    }
}

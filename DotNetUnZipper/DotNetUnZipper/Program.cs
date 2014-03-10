using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

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
                //Checks if any zip files are in that directory
                if (files.Length > 0)
                {
                    Parallel.ForEach(files, info =>
                    {
                        bool status;
                        //extract files
                        Extract(info, out status);
                        if (status)
                        {
                            Console.WriteLine("Unzipped {0} Successfully", info.Name);
                            //Delete successfuly extracted file
                            info.Delete();
                            Console.WriteLine("Deleted {0} Successfully", info.Name);
                        }
                        else
                        {
                            //If any errors occur in the extraction, dump zip in the archive directory for later processing
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
        }

        private static void Archive(FileInfo file)
        {
            if (file == null) return;
            //build full file path
            var path = Path.Combine(ArchivePath, file.Name);
            //copies zip file to the archived directory
            file.CopyTo(path, true);
            file.Delete();
        }
        private static void Extract(FileInfo file, out bool status)
        {
            bool successStatus = false;
            //declares/assigns output directory
            var outfile = ConfigurationSettings.AppSettings["DestinationDirectory"];
            //Build file file path for extraction
            string fileName = Path.Combine(DirPath, file.Name);
            FastZip fastZip = new FastZip();
            try
            {
                //extracts file to destination directory
                fastZip.ExtractZip(fileName, outfile, null);
                //sets success status
                successStatus = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //fails job
                successStatus = false;
            }
            finally
            {
                //sets out param after all proccessing has finished
                status = successStatus;
            }
        }
    }
}

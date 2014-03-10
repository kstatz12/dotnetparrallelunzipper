﻿using System;
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
        static readonly string Outfile = ConfigurationSettings.AppSettings["DestinationDirectory"];
        private static readonly bool IsNestedFolders = bool.Parse(ConfigurationSettings.AppSettings["IsNestedFolders"]);
        static void Main(string[] args)
        {
            var di = new DirectoryInfo(DirPath);
            var files = di.GetFiles("*.zip");
            try
            {
                //Checks if any zip files are in that directory
                if (files.Length > 0)
                {
                    Parallel.ForEach(files, info =>
                    {
                        bool status;
                        //extract files
                        Extract(info, IsNestedFolders, out status);
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
            Console.ReadKey();
        }
        private static int CheckFileIfExists(FileInfo file)
        {
            var di = new DirectoryInfo(Outfile);
            var files = di.GetFiles(file.Name);
            return (int) (files.Length < 0 ? 0 : files.Length);
        }
        private static string CreateDirectory(FileInfo file)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
            string newDirectory = Path.Combine(Outfile, fileNameWithoutExtension);
            if (!Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(newDirectory);
            }
            return newDirectory;
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
        private static void Extract(FileInfo file, bool isNestedFolders, out bool status)
        {
            if (file == null)
            {
                status = false;
            }
            else
            {
                bool successStatus = false;
                //declares/assigns output directory
                //Build file file path for extraction
                string fileName = Path.Combine(DirPath, file.Name);
                FastZip fastZip = new FastZip();
                try
                {
                    if (!isNestedFolders)
                    {
                        var path = CreateDirectory(file);
                        //extracts file to destination directory
                        switch (CheckFileIfExists(file))
                        {
                            case 0:
                                fastZip.ExtractZip(fileName, path, null);
                                successStatus = true;
                                break;
                            default:
                                successStatus = false;
                                throw new Exception("File Already Exists");
                        }
                    }
                    else
                    {
                        switch (CheckFileIfExists(file))
                        {
                            case 0:
                                fastZip.ExtractZip(fileName, Outfile, null);
                                successStatus = true;
                                break;
                            default:
                                successStatus = false;
                                throw new Exception("File Already Exists");
                        }
                    }
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
}

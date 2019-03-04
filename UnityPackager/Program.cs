using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using YamlDotNet.RepresentationModel;

namespace UnityPackager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                Environment.Exit(1);
                return;
            }
            else if (args[0].ToLower() == "pack")
            {
                if (args.Length <= 3)
                {
                    PrintUsage();
                    Environment.Exit(1);
                    return;
                }
                else if ((args.Length - 2) % 2 != 0)
                {
                    PrintUsage();
                    Environment.Exit(1);
                    return;
                }
            
                string outputFile = args[1];

                if (!Path.IsPathRooted(outputFile))
                    outputFile = Path.GetFullPath(outputFile);
            
                List<FileEntry> entries = new List<FileEntry>();
                
                for (int i = 2; i < args.Length; i += 2)
                {
                    FileEntry entry = new FileEntry();

                    if (!Path.IsPathRooted(args[i]))
                        entry.FullPath = Path.GetFullPath(args[i]);
                    else
                        entry.FullPath = args[i];

                    entry.OutputExportPath = args[i + 1];
                
                    entries.Add(entry);
                }

                Pack(entries.ToArray(), outputFile);
            } 
            else if (args[0].ToLower() == "unpack")
            {
                if (args.Length != 3)
                {
                    PrintUsage();
                    Environment.Exit(1);
                    return;
                }
                
                string inputFile = args[1];

                if (!Path.IsPathRooted(inputFile))
                    inputFile = Path.GetFullPath(inputFile);
                
                string outputFolder = args[2];

                if (!Path.IsPathRooted(outputFolder))
                    outputFolder = Path.GetFullPath(outputFolder);
                
                Unpack(inputFile, outputFolder);
            }
            else
            {
                PrintUsage();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\t" + "UnityPackager pack <output> [(<input-file> <target-path>)]...");
            Console.WriteLine("\t" + "UnityPackager unpack <input-file> <output-folder>");
            Console.WriteLine("Example:");
            Console.WriteLine("\t" + "UnityPackager pack MyPackage.unitypackage MyFile.cs Assets/MyFile.cs");
            Console.WriteLine("\t" + "UnityPackager unpack MyPackage.unitypackage MyProjectFolder");
        }
        
        private static void Pack(FileEntry[] files, string outputFile)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            for (int i = 0; i < files.Length; i++)
            {
                string guid = CreateGuid(files[i].OutputExportPath);

                Directory.CreateDirectory(Path.Combine(tempPath, guid));
                File.Copy(files[i].FullPath, Path.Combine(tempPath, guid, "asset"));                
                
                File.WriteAllText(Path.Combine(tempPath, guid, "pathname"), files[i].OutputExportPath);
                
                
                string metaPath = Path.Combine(Path.GetDirectoryName(files[i].FullPath), Path.GetFileNameWithoutExtension(files[i].FullPath) + ".meta");

                if (File.Exists(metaPath))
                {
                    File.Copy(metaPath, Path.Combine(tempPath, guid, "asset.meta"));
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(tempPath, guid, "asset.meta")))
                    {
                        new YamlStream(new YamlDocument(new YamlMappingNode()
                        {
                            {"guid", guid},
                            {"fileFormatVersion", "2"}
                        })).Save(writer);
                    }
                
                    FileInfo metaFile = new FileInfo(Path.Combine(Path.Combine(tempPath, guid, "asset.meta")));
                    
                    using (FileStream metaFileStream = metaFile.Open(FileMode.Open))
                    {
                        metaFileStream.SetLength(metaFile.Length - 3 - Environment.NewLine.Length);
                    }   
                }
            }

            if (File.Exists(outputFile))
                File.Delete(outputFile);
            
            using (FileStream stream = new FileStream(outputFile, FileMode.CreateNew))
            {
                using (GZipOutputStream zipStream = new GZipOutputStream(stream))
                {
                    using (TarArchive archive = TarArchive.CreateOutputTarArchive(zipStream))
                    {
                        archive.RootPath = tempPath.Replace('\\', '/');
                        
                        if (archive.RootPath.EndsWith("/"))
                            archive.RootPath = archive.RootPath.Remove(archive.RootPath.Length - 1);
                        
                        AddFilesInDirRecursive(archive, tempPath);
                    }
                }
            }
            
            // Clean up
            Directory.Delete(tempPath, true);
        }

        private static void Unpack(string inputFile, string outputFolder)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            
            using (FileStream stream = new FileStream(inputFile, FileMode.Open))
            {
                using (GZipInputStream zipStream = new GZipInputStream(stream))
                {
                    using (TarArchive archive = TarArchive.CreateInputTarArchive(zipStream))
                    {
                        archive.ExtractContents(tempPath);
                    }
                }
            }

            string[] dirEntries = Directory.GetDirectories(tempPath);

            for (int i = 0; i < dirEntries.Length; i++)
            {
                if (!File.Exists(Path.Combine(dirEntries[i], "pathname")) || !File.Exists(Path.Combine(dirEntries[i], "asset")) || !File.Exists(Path.Combine(dirEntries[i], "asset.meta")))
                {
                    // Invalid format
                    continue;
                }

                string targetPath = Path.Combine(outputFolder, File.ReadAllText(Path.Combine(dirEntries[i], "pathname")));
                string targetFileNameWithoutExtension = Path.GetFileNameWithoutExtension(targetPath);
                string targetFolder = Path.GetDirectoryName(targetPath);
                string targetMetaPath = Path.Combine(outputFolder, targetFolder, targetFileNameWithoutExtension + ".meta");
                
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                if (File.Exists(targetPath))
                    File.Delete(targetPath);

                if (File.Exists(targetMetaPath))
                    File.Delete(targetMetaPath);
                
                File.WriteAllText(targetPath, File.ReadAllText(Path.Combine(dirEntries[i], "asset")));
                File.WriteAllText(targetMetaPath, File.ReadAllText(Path.Combine(dirEntries[i], "asset.meta")));
            }
            
            // Clean up
            Directory.Delete(tempPath, true);
        }
        
        private static void AddFilesInDirRecursive(TarArchive archive, string directory)
        {
            string[] files = Directory.GetFiles(directory);
            
            for (int i = 0; i < files.Length; i++)
            {
                TarEntry entry = TarEntry.CreateEntryFromFile(files[i]);
                entry.Name = files[i].Remove(0, archive.RootPath.Length + 1).Replace('\\', '/');
                archive.WriteEntry(entry, true);
            }

            string[] subDirs = Directory.GetDirectories(directory);
            
            for (int i = 0; i < subDirs.Length; i++)
            {
                AddFilesInDirRecursive(archive, subDirs[i]);
            }
        }
        
        private static string CreateGuid(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.Unicode.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                
                foreach (byte b in hashBytes)
                {
                    stringBuilder.Append(b.ToString("X2"));
                }
            
                return stringBuilder.ToString();
            }
        }
    }

    public struct FileEntry
    {
        public string FullPath;
        public string OutputExportPath;
    }
}
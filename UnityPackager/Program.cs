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
            if (args.Length < 4) return;

            string inputDir = args[0];
            string outputFile = args[1];
            
            List<FileEntry> entries = new List<FileEntry>();
            for (int i = 2; i < args.Length; i += 2)
            {
                entries.Add(new FileEntry()
                {
                    FullPath = args[i],
                    OutputExportPath = args[i + 1]
                });
            }

            Pack(entries.ToArray(), inputDir, outputFile);
        }

        private static void Pack(FileEntry[] files, string inputDirectory, string outputFile)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            for (int i = 0; i < files.Length; i++)
            {
                string guid = CreateGuid(files[i].OutputExportPath);

                Directory.CreateDirectory(Path.Combine(tempPath, guid));
                File.Copy(files[i].FullPath, Path.Combine(tempPath, guid, "asset"));
                File.WriteAllText(Path.Combine(tempPath, guid, "pathname"), files[i].OutputExportPath);
                
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
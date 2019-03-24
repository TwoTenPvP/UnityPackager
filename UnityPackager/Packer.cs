using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using YamlDotNet.RepresentationModel;

namespace UnityPackager
{
    public static class Packer
    {
        public static void Pack(IDictionary<string,string> files, string outputFile)
        {
            string randomFile = Path.GetRandomFileName();

            string tempPath = Path.Combine(Path.GetTempPath(), randomFile);
            Directory.CreateDirectory(tempPath);
            AddAssets(files, tempPath);

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            Compress(outputFile, tempPath);

            // Clean up
            Directory.Delete(tempPath, true);
        }

        private static void AddAssets(IDictionary<string, string> files, string tempPath)
        {
            foreach (KeyValuePair<string, string> fileEntry in files)
            {
                if (File.Exists(fileEntry.Key))
                    AddAsset(tempPath, fileEntry.Key, fileEntry.Value);
                else if (Directory.Exists(fileEntry.Key))
                    AddFolder(tempPath, fileEntry.Key, fileEntry.Value);
                else
                    throw new FileNotFoundException($"Could not find file or directory {fileEntry.Key}");
            }
        }

        private static void AddFolder(string tempPath, string folder, string destination)
        {
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

            foreach (string filename in files)
            {
                // metas will be copied with their asset
                if (Path.GetExtension(filename) == ".meta")
                    continue;

                string destinationPath = Path.Combine(destination, Path.GetRelativePath(folder, filename));

                AddAsset(tempPath, filename, destinationPath);
            }
        }

        private static void AddAsset(string tempPath, string fromFile, string toPath)
        {
            YamlDocument meta = GetMeta(fromFile) ?? GenerateMeta(toPath);

            string guid = GetGuid(meta);

            Directory.CreateDirectory(Path.Combine(tempPath, guid));

            string assetPath = Path.Combine(tempPath, guid, "asset");
            File.Copy(fromFile, assetPath);

            string pathnamePath = Path.Combine(tempPath, guid, "pathname");
            File.WriteAllText(pathnamePath, toPath);

            string metaPath = Path.Combine(tempPath, guid, "asset.meta");
            SaveMeta(metaPath, meta);
        }

        private static void SaveMeta(string metaPath, YamlDocument meta)
        {
            using (StreamWriter writer = new StreamWriter(metaPath))
            {
                new YamlStream(meta).Save(writer);
            }

            FileInfo metaFile = new FileInfo(metaPath);

            using (FileStream metaFileStream = metaFile.Open(FileMode.Open))
            {
                metaFileStream.SetLength(metaFile.Length - 3 - Environment.NewLine.Length);
            }
        }

        private static string GetGuid(YamlDocument meta)
        {
            YamlMappingNode mapping = (YamlMappingNode)meta.RootNode;

            YamlScalarNode key = new YamlScalarNode("guid");

            YamlScalarNode value = (YamlScalarNode)mapping[key];
            return value.Value;
        }

        private static YamlDocument GenerateMeta(string filename)
        {
            string guid = Utils.CreateGuid(filename);

            return new YamlDocument(new YamlMappingNode
                        {
                            {"guid", guid},
                            {"fileFormatVersion", "2"}
                        });
        }

        private static YamlDocument GetMeta(string filename)
        {
            // do we have a .meta file?
            string metaPath = filename + ".meta";

            if (!File.Exists(metaPath))
                return null;

            using (StreamReader reader = new StreamReader(metaPath))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                return yaml.Documents[0];
            }
        }

        private static void Compress(string outputFile, string tempPath)
        {
            using (FileStream stream = new FileStream(outputFile, FileMode.CreateNew))
            {
                using (GZipOutputStream zipStream = new GZipOutputStream(stream))
                {
                    using (TarArchive archive = TarArchive.CreateOutputTarArchive(zipStream))
                    {
                        archive.RootPath = tempPath;
                        archive.AddFilesRecursive(tempPath);
                    }
                }
            }
        }
    }
}

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
                string guid = Utils.CreateGuid(fileEntry.Value);

                Directory.CreateDirectory(Path.Combine(tempPath, guid));
                File.Copy(fileEntry.Key, Path.Combine(tempPath, guid, "asset"));

                File.WriteAllText(Path.Combine(tempPath, guid, "pathname"), fileEntry.Value);

                string metaPath = Path.Combine(Path.GetDirectoryName(fileEntry.Key), Path.GetFileNameWithoutExtension(fileEntry.Key) + ".meta");

                if (File.Exists(metaPath))
                {
                    File.Copy(metaPath, Path.Combine(tempPath, guid, "asset.meta"));
                }
                else
                {
                    // TODO: If a meta file exists, grab it instead of creating this barebones version.
                    // Requires research on how Unity does it, to fully mimic the real packing behaviour
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

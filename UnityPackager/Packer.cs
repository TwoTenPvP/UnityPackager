using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using YamlDotNet.RepresentationModel;

namespace UnityPackager
{
    public static class Packer
    {
        public static void Pack(FileEntry[] files, string outputFile)
        {
            string randomFile = Path.GetRandomFileName();

            string tempPath = Path.Combine(Path.GetTempPath(), randomFile);
            Directory.CreateDirectory(tempPath);

            for (int i = 0; i < files.Length; i++)
            {
                string guid = Utils.CreateGuid(files[i].OutputExportPath);

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

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            Compress(outputFile, tempPath);

            // Clean up
            Directory.Delete(tempPath, true);
        }

        private static void Compress(string outputFile, string tempPath)
        {
            using (FileStream stream = new FileStream(outputFile, FileMode.CreateNew))
            {
                using (GZipOutputStream zipStream = new GZipOutputStream(stream))
                {
                    using (TarArchive archive = TarArchive.CreateOutputTarArchive(zipStream))
                    {
                        archive.RootPath = tempPath.Replace('\\', '/');

                        if (archive.RootPath.EndsWith("/"))
                            archive.RootPath = archive.RootPath.Remove(archive.RootPath.Length - 1);

                        Archive.AddFilesRecursive(archive, tempPath);
                    }
                }
            }
        }
    }
}

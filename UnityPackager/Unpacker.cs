using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityPackager
{
    public static class Unpacker
    {
        public static void Unpack(string inputFile, string outputFolder)
        {
            string fileName = Path.GetRandomFileName();
            string tempPath = Path.Combine(Path.GetTempPath(), fileName);
            Directory.CreateDirectory(tempPath);

            Decompress(inputFile, tempPath);

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
                string targetMetaPath = Path.Combine(targetFolder, targetFileNameWithoutExtension + ".meta");

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

        private static void Decompress(string inputFile, string tempPath)
        {
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
        }
    }
}

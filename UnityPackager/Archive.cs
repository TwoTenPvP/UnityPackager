using System;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityPackager
{
    public class Archive
    {
        /// <summary>
        /// Tar a folder recursively
        /// </summary>
        /// <param name="archive">Archive.</param>
        /// <param name="directory">Directory.</param>
        public static void AddFilesInDirRecursive(TarArchive archive, string directory)
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
    }
}

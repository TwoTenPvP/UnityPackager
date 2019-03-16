using System.IO;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityPackager
{
    public static class Archive
    {
        /// <summary>
        /// Tar a folder recursively
        /// </summary>
        /// <param name="archive">Archive.</param>
        /// <param name="directory">Directory.</param>
        public static void AddFilesRecursive(this TarArchive archive, string directory)
        {
            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

            foreach (string filename in files)
            {
                TarEntry entry = TarEntry.CreateEntryFromFile(filename);
                entry.Name = entry.Name.Replace('\\', '/');
                archive.WriteEntry(entry, true);
            }
        }
    }
}
